namespace WorkerHost
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure;
    using Microsoft.ConnectTheDots.Common;
    using Microsoft.ConnectTheDots.Gateway;
    using Microsoft.WindowsAzure.ServiceRuntime;
    using Newtonsoft.Json;

    using ApiReaders;
    using Data.Repositories;
    using Data.Contracts;

    public class WorkerHost : RoleEntryPoint
    {
        private static readonly ILogger _logger = EventLogger.Instance;

        private static readonly DataCache _cache = new DataCache();

        private static DataValueRepository _FlowDataRepository;
        private static DataSourceRepository _FlowSourceRepository;

        private static GatewayService flowGateway;
        private static GatewayService devicesGateway;
        private static GatewayService xmlApiGateway;

        private enum _UpdateType
        {
            FlowValue = 0,
            FlowSource = 1
        }
        private static readonly ConcurrentQueue<KeyValuePair<_UpdateType, ApiDataContract>> _UpdateQueue
             = new ConcurrentQueue<KeyValuePair<_UpdateType, ApiDataContract>>();
        
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

        public static void OnXMLApiData(XMLApiDefinition definitionForData, XMLApiData data)
        {
            DateTime currentTime = DateTime.UtcNow;
            EHRawXMLApiDataContract ehData = new EHRawXMLApiDataContract
            {
                StationID = data.id,
                Route = definitionForData.Route,
                Direction = definitionForData.Direction,
                Milepost = definitionForData.Milepost,
                Location = definitionForData.Location,
                Volume = data.Volume,
                Occupancy = data.Occupancy,
                Speed = data.Speed,
                TimeCreated = currentTime
            };
            xmlApiGateway.Enqueue(JsonConvert.SerializeObject(ehData));
        }

        private static void StartXMLApiProcesses()
        {
            var configItems = Loader.GetAPIConfigItems();
            foreach (var config in configItems)
            {
                XMLApiReaderProcess<string, XMLApiDefinition, XMLApiData> process =
                new XMLApiReaderProcess<string, XMLApiDefinition, XMLApiData>(
                    config.DefinitionAddress, config.DefinitionXMLRootNodeName,
                    config.APIAddress, config.DataXMLRootNodeName,
                    config.IntervalSecs, OnXMLApiData
                );
                process.Start();
            }
        }

        private static void StartHost()
        {
            bool sendToEHRaw = CloudConfigurationManager.GetSetting("sendToEHRaw").ToLowerInvariant() == "true";
            bool sendToEHDevices = CloudConfigurationManager.GetSetting("sendToEHDevices").ToLowerInvariant() == "true";
            bool sendToSQL = CloudConfigurationManager.GetSetting("sendToSQL").ToLowerInvariant() == "true";
            bool sendXMLApiRawEH = CloudConfigurationManager.GetSetting("sendXMLApiRawEH").ToLowerInvariant() == "true";

            if (sendToSQL)
            {
                string sqlDatabaseConnectionString = CloudConfigurationManager.GetSetting("sqlDatabaseConnectionString");

                _FlowDataRepository = new DataValueRepository(sqlDatabaseConnectionString);
                _FlowSourceRepository = new DataSourceRepository(sqlDatabaseConnectionString);

                Task.Run(() => UpdateQueueProcessor());
            }

            _logger.LogInfo("Starting Worker...");

            const int SLEEP_TIME_MS = 60000;//1 minute

            //gateway for raw data
            AMQPConfig amqpRAWConfig = Loader.GetAMQPConfig("RawDataAMQPConfig", _logger);              // set to ehraw in config file
            flowGateway = CreateGateway(amqpRAWConfig);
            
            //gateway for CTD-style json formatted data
            AMQPConfig amqpDevicesConfig = Loader.GetAMQPConfig("DevicesAMQPConfig", _logger);          // set to ehdevices in config file
            devicesGateway = CreateGateway(amqpDevicesConfig);

            AMQPConfig amqpXMLApiConfig = Loader.GetAMQPConfig("XMLApiAMQPConfig", _logger);          // set to ehraw in config file
            xmlApiGateway = CreateGateway(amqpXMLApiConfig);

            if (sendXMLApiRawEH)
            {
                Task.Run(() => StartXMLApiProcesses());
            }

            string url = CloudConfigurationManager.GetSetting("ApiUrl") + CloudConfigurationManager.GetSetting("AccessCode");
            ApiReader test = new ApiReader(url);

            for (; ; )
            {
                try
                {
                    IEnumerable<ApiDataContract> list = test.GetTrafficFlowsAsJson<ApiDataContract>().Result;

                    foreach (ApiDataContract data in list)
                    {
                        EHRawDataContract messageRaw = new EHRawDataContract
                        {
                            DataID = data.DataID.ToString(),
                            Region = data.Region,
                            StationName = data.StationName,
                            LocationDescription = data.StationLocation.Description,
                            Direction = data.StationLocation.Direction,
                            Latitude = data.StationLocation.Latitude.ToString(),
                            Longitude = data.StationLocation.Longitude.ToString(),
                            MilePost = data.StationLocation.MilePost.ToString(),
                            RoadName = data.StationLocation.RoadName,
                            Value = data.ReadingValue,
                            TimeCreated = data.Time
                        };
                        

                        bool updateDataValue, updateDataSource;
                        _cache.Set(data, out updateDataValue, out updateDataSource);

                        if (sendToEHRaw)
                        {
                            flowGateway.Enqueue(JsonConvert.SerializeObject(messageRaw));
                        }
                        
                        if (sendToEHDevices && updateDataValue)
                        {
                            // sending JSON received and reformated as CTD format to ehdevices.

                            SensorDataContract message = new SensorDataContract
                            {

                                Guid = data.DataID.ToString(),
                                DisplayName = data.StationLocation.Description,
                                MeasureName = data.StationLocation.RoadName,
                                UnitOfMeasure = "Flow on " + data.StationLocation.RoadName,
                                Location =
                                    data.StationLocation.RoadName + "\n" + data.StationLocation.Latitude +
                                    " " + data.StationLocation.Longitude,
                                Value = data.ReadingValue,
                                TimeCreated = data.Time,
                                Organization = ""
                            };
                            devicesGateway.Enqueue(JsonConvert.SerializeObject(message));                                
                        }

                        if (sendToSQL)
                        {
                            if (updateDataValue)
                            {
                                _UpdateQueue.Enqueue(new KeyValuePair<_UpdateType, ApiDataContract>(_UpdateType.FlowValue, data));
                            }
                            if (updateDataSource)
                            {
                                _UpdateQueue.Enqueue(new KeyValuePair<_UpdateType, ApiDataContract>(_UpdateType.FlowSource, data));
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
                        List<ApiDataContract> updateValueList = new List<ApiDataContract>();
                        List<ApiDataContract> updateSourcesList = new List<ApiDataContract>();

                        for (int dequeueTry = 0; dequeueTry < count; ++dequeueTry)
                        {
                            KeyValuePair<_UpdateType, ApiDataContract> updatePair;
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
                        _FlowSourceRepository.ProcessEvents(updateSourcesList);
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

                var _AMPQSender = new AMQPSender<EHRawDataContract>(
                                                    amqpConfig.AMQPSAddress,
                                                    amqpConfig.EventHubName,
                                                    amqpConfig.EventHubMessageSubject,
                                                    amqpConfig.EventHubDeviceId,
                                                    amqpConfig.EventHubDeviceDisplayName,
                                                    _logger
                                                    );

                var _batchSenderThread = new BatchSenderThread<QueuedItem, EHRawDataContract>(
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
