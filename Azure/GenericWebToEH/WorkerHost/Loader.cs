using System;
using System.Collections.Generic;
using System.Linq;
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
