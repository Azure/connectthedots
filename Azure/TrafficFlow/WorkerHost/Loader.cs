using System;
using Microsoft.ConnectTheDots.Common;

namespace WorkerHost
{
    using System.Configuration;

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
        internal static AMQPConfig GetAMQPConfig(string configSection, ILogger logger)
        {
            AMQPConfig configData = null;
            try
            {
                AMQPServiceConfigSection section =
                    ConfigurationManager.GetSection(configSection) as AMQPServiceConfigSection;

                if (section != null)
                {
                    configData = new AMQPConfig
                    {
                        AMQPSAddress = section.AMQPSAddress,
                        EventHubName = section.EventHubName,
                        EventHubMessageSubject = section.EventHubMessageSubject,
                        EventHubDeviceId = section.EventHubDeviceId,
                        EventHubDeviceDisplayName = section.EventHubDeviceDisplayName
                    };
                }
            }
            catch(Exception ex)
            {
                logger.LogError(ex.Message);
            }

            return configData;
        }
    }

    internal class AMQPServiceConfigSection : ConfigurationSection
    {
        [ConfigurationProperty("AMQPSAddress", DefaultValue = "AMQPSAddress", IsRequired = true)]
        public string AMQPSAddress
        {
            get
            {
                return (string)this["AMQPSAddress"];
            }
            set
            {
                this["AMQPSAddress"] = value;
            }
        }

        [ConfigurationProperty("EventHubName", DefaultValue = "EventHubName", IsRequired = true)]
        public string EventHubName
        {
            get
            {
                return (string)this["EventHubName"];
            }
            set
            {
                this["EventHubName"] = value;
            }
        }

        [ConfigurationProperty("EventHubMessageSubject", DefaultValue = "EventHubMessageSubject", IsRequired = true)]
        public string EventHubMessageSubject
        {
            get
            {
                return (string)this["EventHubMessageSubject"];
            }
            set
            {
                this["EventHubMessageSubject"] = value;
            }
        }

        [ConfigurationProperty("EventHubDeviceId", DefaultValue = "EventHubDeviceId", IsRequired = true)]
        public string EventHubDeviceId
        {
            get
            {
                return (string)this["EventHubDeviceId"];
            }
            set
            {
                this["EventHubDeviceId"] = value;
            }
        }

        [ConfigurationProperty("EventHubDeviceDisplayName", DefaultValue = "EventHubDeviceDisplayName", IsRequired = true)]
        public string EventHubDeviceDisplayName
        {
            get
            {
                return (string)this["EventHubDeviceDisplayName"];
            }
            set
            {
                this["EventHubDeviceDisplayName"] = value;
            }
        }
    }
}
