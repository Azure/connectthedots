using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.Azure;
using Microsoft.ConnectTheDots.Common;

namespace WorkerHost
{
    using System.Configuration;

    public class AppConfiguration
    {
        public int SleepTimeMs;

        public NetworkCredential CredentialToUse;

        public bool UseXml;

        public string XmlTemplate;

        public string ServiceBusConnectionString;
        public string EventHubName;

        public string MessageSubject;
        public string MessageDeviceId;
        public string MessageDeviceDisplayName;
    }

    public class AMQPConfig
    {
        public string AMQPSAddress;
        public string EventHubName;
        public string EventHubMessageSubject;
        public string EventHubDeviceId;
        public string EventHubDeviceDisplayName;
    };

    public class Loader
    {
        public static IList<XMLApiConfigItem> GetAPIConfigItems()
        {
            var result = new List<XMLApiConfigItem>();

            XMLApiListConfigSection config = ConfigurationManager.GetSection("XMLApiListConfig") as XMLApiListConfigSection;

            if (config != null)
            {
                result.AddRange(config.Instances.Cast<XMLApiConfigItem>());
            }

            return result;
        }

        public static AppConfiguration GetConfig(ILogger logger)
        {
            AppConfiguration config = new AppConfiguration
            {
                CredentialToUse = new NetworkCredential(CloudConfigurationManager.GetSetting("UserName"),
                CloudConfigurationManager.GetSetting("Password")),
                UseXml = CloudConfigurationManager.GetSetting("SendJson").ToLowerInvariant().Contains("false"),
                XmlTemplate = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).GetSection("MergeToXML").SectionInformation.GetRawXml(),
                ServiceBusConnectionString = ReadConfigValue("Microsoft.ServiceBus.ServiceBusConnectionString", "[Service Bus connection string]"),
                EventHubName = ReadConfigValue("Microsoft.ServiceBus.EventHubToUse", "[event hub name]"),
                MessageSubject = CloudConfigurationManager.GetSetting("MessageSubject"),
                MessageDeviceId = CloudConfigurationManager.GetSetting("MessageDeviceId"),
                MessageDeviceDisplayName = CloudConfigurationManager.GetSetting("MessageDeviceDisplayName")
            };
            if (!int.TryParse(CloudConfigurationManager.GetSetting("SleepTimeMs"), out config.SleepTimeMs))
            {
                logger.LogInfo("Incorrect SleepTimeMs value, using default...");
                //default sleep time interval is 10 sec
                config.SleepTimeMs = 10000;
            }

            return config;
        }
        private static string ReadConfigValue(string keyName, string defaultNotSetValue)
        {
            string value = CloudConfigurationManager.GetSetting(keyName);
            if (string.IsNullOrEmpty(value) || value.Equals(defaultNotSetValue))
            {
                value = ConfigurationManager.AppSettings.Get(keyName);
            }
            return value;
        }
    }

    public class XMLApiListConfigSection : ConfigurationSection
    {
        [ConfigurationProperty("", IsRequired = true, IsDefaultCollection = true)]
        public APIListConfigInstanceCollection Instances
        {
            get { return (APIListConfigInstanceCollection)this[""]; }
            set { this[""] = value; }
        }
    }

    public class APIListConfigInstanceCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new XMLApiConfigItem();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((XMLApiConfigItem)element).APIAddress;
        }
    }

    public class XMLApiConfigItem : ConfigurationElement
    {
        [ConfigurationProperty("APIAddress", IsKey = true, IsRequired = true)]
        public string APIAddress
        {
            get
            {
                return (string)base["APIAddress"];
            }
        }
    }
}
