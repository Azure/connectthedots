namespace WorkerHost
{
    using System;
    using System.Configuration;
    using System.Threading;
    using Microsoft.Azure;
    using Microsoft.ConnectTheDots.Common;
    using Microsoft.ConnectTheDots.Gateway;
    using Microsoft.WindowsAzure.ServiceRuntime;
    using System.Collections.Generic;
    using System.Net;

    using ApiReaders;

    public class WorkerHost : RoleEntryPoint
    {
        private static readonly ILogger _logger = EventLogger.Instance;

        private static GatewayService gateway;

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
            _logger.LogInfo("Starting Worker...");

            const int SLEEP_TIME_MS = 10000;

            
            
            NetworkCredential credentialToUse = new NetworkCredential(CloudConfigurationManager.GetSetting("UserName"),
                CloudConfigurationManager.GetSetting("Password"));

            bool useXML = CloudConfigurationManager.GetSetting("SendJson").ToLowerInvariant().Contains("false");

            var xmlTemplate = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).GetSection("MergeToXML").SectionInformation.GetRawXml();

            var readers = PrepareReaders(xmlTemplate, useXML, credentialToUse);

            AMQPConfig amqpDevicesConfig = Loader.GetAMQPConfig("TargetAMQPConfig", _logger);
            gateway = CreateGateway(amqpDevicesConfig);

            for (; ; )
            {
                try
                {
                    foreach (var reader in readers)
                    {

                        IEnumerable<string> dataEnumerable = reader.GetData();
                        foreach (string newDataJson in dataEnumerable)
                        {
                            if (newDataJson != null)
                            {
                                gateway.Enqueue(newDataJson);
                            }
                        }
                    }

                    Thread.Sleep(SLEEP_TIME_MS);
                }
                catch(Exception ex)
                {
                    Thread.Sleep(SLEEP_TIME_MS/2);
                    _logger.LogError(ex.Message);
                }
            }
        }

        private static IEnumerable<RawXMLWithHeaderToJsonReader> PrepareReaders(string xmlTemplate, bool useXML, NetworkCredential credeitial)
        {
            List<RawXMLWithHeaderToJsonReader> result = new List<RawXMLWithHeaderToJsonReader>();
            var configItems = Loader.GetAPIConfigItems();
            foreach (var config in configItems)
            {
                result.Add(new RawXMLWithHeaderToJsonReader(xmlTemplate, useXML, config.APIAddress, credeitial));
            }
            return result;
        }

        private static GatewayService CreateGateway(AMQPConfig amqpConfig)
        {
            try
            {
                var _gatewayQueue = new GatewayQueue<QueuedItem>();

                var _AMPQSender = new AMQPSender<string>(
                                                    amqpConfig.AMQPSAddress,
                                                    amqpConfig.EventHubName,
                                                    amqpConfig.EventHubMessageSubject,
                                                    amqpConfig.EventHubDeviceId,
                                                    amqpConfig.EventHubDeviceDisplayName,
                                                    _logger
                                                    );

                var _batchSenderThread = new BatchSenderThread<QueuedItem, string>(
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
