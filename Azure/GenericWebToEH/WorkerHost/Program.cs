using System.Threading.Tasks;

namespace WorkerHost
{
    using System;
    using System.Configuration;
    using System.Threading;
    using Microsoft.Azure;
    using Microsoft.ConnectTheDots.Common;
    using Microsoft.ConnectTheDots.Gateway;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using Microsoft.WindowsAzure.ServiceRuntime;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;

    using ApiReaders;

    public class WorkerHost : RoleEntryPoint
    {
        private static readonly ILogger _Logger = EventLogger.Instance;
        private static GatewayService _Gateway;
        private static IEnumerable<RawXMLWithHeaderToJsonReader> _Readers;

        private static AppConfiguration _Config;
        

        static void Main()
        {
            try
            {
                StartHost();
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex.Message);
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
                _Logger.LogError(ex.Message);
            }
        }

        public override bool OnStart()
        {
            RoleEnvironment.Changed += RoleEnvironmentChanging;

            return base.OnStart();
        }

        private void RoleEnvironmentChanging(object sender, RoleEnvironmentChangedEventArgs e)
        {
            if (!e.Changes.Any(change => change is RoleEnvironmentConfigurationSettingChange)) return;

            InitWorkerConfiguration();
        }

        private static void StartHost()
        {
            _Logger.LogInfo("Starting Worker...");

            InitWorkerConfiguration();

            Process();
        }

        private static void InitWorkerConfiguration()
        {
            AppConfiguration config = Loader.GetConfig(_Logger);
            IEnumerable<RawXMLWithHeaderToJsonReader> readers = PrepareReaders(config.XmlTemplate, config.UseXml, config.CredentialToUse);

            AMQPConfig amqpDevicesConfig = PrepareAMQPConfig(config.ServiceBusConnectionString,
                config.EventHubName,
                config.MessageSubject,
                config.MessageDeviceId,
                config.MessageDeviceDisplayName);

            if (amqpDevicesConfig == null)
            {
                Interlocked.Exchange(ref _Gateway, null);
                Interlocked.Exchange(ref _Readers, null);
                Interlocked.Exchange(ref _Config, config);
                _Logger.LogInfo("Not able to construct AMQP config for Event Hub using provided connection string...");
                return;
            }
            Interlocked.Exchange(ref _Gateway, CreateGateway(amqpDevicesConfig));
            Interlocked.Exchange(ref _Readers, readers);
            Interlocked.Exchange(ref _Config, config);
        }

        private static void Process()
        {
            const int SLEEP_TIME_ON_EXCEPTION_MS = 5000;
            for (; ; )
            {
                try
                {
                    if (_Readers != null)
                    {
                        foreach (var reader in _Readers)
                        {
                            IEnumerable<string> dataEnumerable = reader.GetData();
                            foreach (string newDataJson in dataEnumerable)
                            {
                                if (_Gateway != null && newDataJson != null)
                                {
                                    _Gateway.Enqueue(newDataJson);
                                }
                            }
                        }
                    }

                    Thread.Sleep(_Config.SleepTimeMs);
                }
                catch (Exception ex)
                {
                    _Logger.LogError(ex.Message);
                    Thread.Sleep(SLEEP_TIME_ON_EXCEPTION_MS);
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

        private static AMQPConfig PrepareAMQPConfig(string connectionString, string hubName,
            string messageSubject, string messageDeviceId, string messageDeviceDisplayName)
        {
            try
            {
                NamespaceManager nsmgr = NamespaceManager.CreateFromConnectionString(connectionString);
                EventHubDescription desc = nsmgr.GetEventHub(hubName);

                foreach (var rule in desc.Authorization)
                {
                    var accessAuthorizationRule = rule as SharedAccessAuthorizationRule;
                    if (accessAuthorizationRule == null) continue;
                    if (!accessAuthorizationRule.Rights.Contains(AccessRights.Send)) continue;

                    string amqpAddress = string.Format("amqps://{0}:{1}@{2}",
                        accessAuthorizationRule.KeyName,
                        Uri.EscapeDataString(accessAuthorizationRule.PrimaryKey), nsmgr.Address.Host);

                    AMQPConfig amqpConfig = new AMQPConfig
                    {
                        AMQPSAddress = amqpAddress,
                        EventHubName = hubName,
                        EventHubDeviceDisplayName =
                            string.IsNullOrEmpty(messageSubject) ? "SensorGatewayService" : messageSubject,
                        EventHubDeviceId =
                            string.IsNullOrEmpty(messageDeviceId)
                                ? "a94cd58f-4698-4d6a-b9b5-4e3e0f794618"
                                : messageDeviceId,
                        EventHubMessageSubject =
                            string.IsNullOrEmpty(messageDeviceDisplayName) ? "gtsv" : messageDeviceDisplayName
                    };
                    return amqpConfig;
                }
            }
            catch (Exception)
            {
            }
            return null;
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
                                                    _Logger
                                                    );

                var _batchSenderThread = new BatchSenderThread<QueuedItem, string>(
                                                    _gatewayQueue,
                                                    _AMPQSender,
                                                    null,
                                                    m => m.JsonData,
                                                    _Logger);

                _batchSenderThread.Start();

                GatewayService service = new GatewayService(
                    _gatewayQueue,
                    _batchSenderThread
                )
                {
                    Logger = _Logger
                };

                service.OnDataInQueue += (data) => _batchSenderThread.Process();
                _Logger.Flush();

                return service;
            }
            catch (Exception ex)
            {
                _Logger.LogError("Exception on creating Gateway: " + ex.Message);
            }

            return null;
        }
    }
}
