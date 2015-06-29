namespace WorkerHost
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Concurrent;
    
    using System.Configuration;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure;
    using Microsoft.ConnectTheDots.Common;
    using Microsoft.ConnectTheDots.Gateway;
    using Microsoft.WindowsAzure.ServiceRuntime;
    using Newtonsoft.Json;

    using TrafficFlow.Common;
    using TrafficFlow.Common.Repositories;

    public class WorkerHost : RoleEntryPoint
    {
        private static readonly ILogger _logger = EventLogger.Instance;

        private static readonly FlowCache _cache = new FlowCache();

        private static FlowDataRepository _FlowDataRepository;
        private static FlowSourcesRepository _FlowSourcesRepository;

        private enum _UpdateType
        {
            FlowValue = 0,
            FlowSource = 1
        }
        private static readonly ConcurrentQueue<KeyValuePair<_UpdateType, Flow>> _UpdateQueue
             = new ConcurrentQueue<KeyValuePair<_UpdateType, Flow>>();
        
        static void Main()
        {
            try
            {
                StartHost();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        public override void Run()
        {
            try
            {
                StartHost();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                
            }
        }

        private static void StartHost()
        {
            bool sendToEHRaw = CloudConfigurationManager.GetSetting("sendToEHRaw").ToLowerInvariant() == "true";
            bool sendToEHDevices = CloudConfigurationManager.GetSetting("sendToEHDevices").ToLowerInvariant() == "true";
            bool sendToSQL = CloudConfigurationManager.GetSetting("sendToSQL").ToLowerInvariant() == "true";

            if (sendToSQL)
            {
                string sqlDatabaseConnectionString = CloudConfigurationManager.GetSetting("sqlDatabaseConnectionString");

                _FlowDataRepository = new FlowDataRepository(sqlDatabaseConnectionString);
                _FlowSourcesRepository = new FlowSourcesRepository(sqlDatabaseConnectionString);

                Task.Run(() => UpdateQueueProcessor());
            }

            _logger.LogInfo("Starting Worker...");

            const int SLEEP_TIME_MS = 60000;//1 minute

            //gateway for Flow-formatted data
            AMQPConfig amqpRAWConfig = Loader.GetAMQPConfig("RawDataAMQPConfig", _logger);              // set to ehraw in config file
            GatewayService flowGateway = CreateGateway(amqpRAWConfig);

            //gateway for CTD-style json formatted data
            AMQPConfig amqpDevicesConfig = Loader.GetAMQPConfig("DevicesAMQPConfig", _logger);          // set to ehdevices in config file
            GatewayService devicesGateway = CreateGateway(amqpDevicesConfig);

            string url = CloudConfigurationManager.GetSetting("ApiUrl") + CloudConfigurationManager.GetSetting("AccessCode");
            ApiReader test = new ApiReader(url);

            for (; ; )
            {
                try
                {
                    IEnumerable<Flow> list = test.GetTrafficFlowsAsJson().Result;

                    foreach (Flow flow in list)
                    {
                        FlowEHDataContract messageRaw = new FlowEHDataContract
                        {
                            FlowDataID = flow.FlowDataID.ToString(),
                            Region = flow.Region,
                            StationName = flow.StationName,
                            LocationDescription = flow.FlowStationLocation.Description,
                            Direction = flow.FlowStationLocation.Direction,
                            Latitude = flow.FlowStationLocation.Latitude.ToString(),
                            Longitude = flow.FlowStationLocation.Longitude.ToString(),
                            MilePost = flow.FlowStationLocation.MilePost.ToString(),
                            RoadName = flow.FlowStationLocation.RoadName,
                            Value = flow.FlowReadingValue,
                            TimeCreated = flow.Time
                        };
                        

                        bool updateFlowValue, updateFlowSource;
                        _cache.Set(flow, out updateFlowValue, out updateFlowSource);

                        if (sendToEHRaw)
                        {
                            flowGateway.Enqueue(JsonConvert.SerializeObject(messageRaw));
                        }
                        
                        if (sendToEHDevices && updateFlowValue)
                        {
                            // sending JSON received and reformated as CTD format to ehdevices.

                            SensorDataContract message = new SensorDataContract
                            {

                                Guid = flow.FlowDataID.ToString(),
                                DisplayName = flow.FlowStationLocation.Description,
                                MeasureName = flow.FlowStationLocation.RoadName,
                                UnitOfMeasure = "Flow on " + flow.FlowStationLocation.RoadName,
                                Location =
                                    flow.FlowStationLocation.RoadName + "\n" + flow.FlowStationLocation.Latitude +
                                    " " + flow.FlowStationLocation.Longitude,
                                Value = flow.FlowReadingValue,
                                TimeCreated = flow.Time,
                                Organization = ""
                            };
                            devicesGateway.Enqueue(JsonConvert.SerializeObject(message));                                
                        }

                        if (sendToSQL)
                        {
                            if (updateFlowValue)
                            {
                                _UpdateQueue.Enqueue(new KeyValuePair<_UpdateType, Flow>(_UpdateType.FlowValue, flow));
                            }
                            if (updateFlowSource)
                            {
                                _UpdateQueue.Enqueue(new KeyValuePair<_UpdateType, Flow>(_UpdateType.FlowSource, flow));
                            }
                        }
                    }

                    Thread.Sleep(SLEEP_TIME_MS);
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
            }
        }

        public static void UpdateQueueProcessor()
        {
            const int SLEEP_TIME_MS = 100;
            const int BATCH_SIZE = 90;

            for (;;)
            {
                try
                {
                    int count = Math.Min(_UpdateQueue.Count, BATCH_SIZE);

                    if (count > 0)
                    {
                        List<Flow> updateValueList = new List<Flow>();
                        List<Flow> updateSourcesList = new List<Flow>();

                        for (int dequeueTry = 0; dequeueTry < count; ++dequeueTry)
                        {
                            KeyValuePair<_UpdateType, Flow> updatePair;
                            if (_UpdateQueue.TryDequeue(out updatePair))
                            {
                                switch (updatePair.Key)
                                {
                                    case _UpdateType.FlowValue:
                                        updateValueList.Add(updatePair.Value);
                                        break;
                                    case _UpdateType.FlowSource:
                                        updateSourcesList.Add(updatePair.Value);
                                        break;
                                }
                            }
                        }
                        _FlowSourcesRepository.ProcessEvents(updateSourcesList);
                        _FlowDataRepository.ProcessEvents(updateValueList);
                    }
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex.Message);
                }

                Thread.Sleep(SLEEP_TIME_MS);
            }
        }

        public static GatewayService CreateGateway(AMQPConfig amqpConfig)
        {
            try
            {
                var _gatewayQueue = new GatewayQueue<QueuedItem>();

                var _AMPQSender = new AMQPSender<TrafficFlow.Common.FlowEHDataContract>(
                                                    amqpConfig.AMQPSAddress,
                                                    amqpConfig.EventHubName,
                                                    amqpConfig.EventHubMessageSubject,
                                                    amqpConfig.EventHubDeviceId,
                                                    amqpConfig.EventHubDeviceDisplayName,
                                                    _logger
                                                    );

                var _batchSenderThread = new BatchSenderThread<QueuedItem, TrafficFlow.Common.FlowEHDataContract>(
                                                    _gatewayQueue,
                                                    _AMPQSender,
                                                    null,
                                                    m => m.JsonData,
                                                    _logger);

                _batchSenderThread.Start();

                GatewayService service = new GatewayService(
                    _gatewayQueue,
                    _batchSenderThread
                )
                {
                    Logger = _logger
                };

                service.OnDataInQueue += (data) => _batchSenderThread.Process();
                _logger.Flush();

                return service;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception on creating Gateway: " + ex.Message);
            }

            return null;
        }
    }
}
