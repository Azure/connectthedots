using System.Collections.Generic;
using System.Configuration;


namespace WorkerHost
{

    public class MailToConfigSection : ConfigurationSection
    {
        [ConfigurationProperty("", IsRequired = true, IsDefaultCollection = true)]
        public MailToConfigInstanceCollection Instances
        {
            get { return (MailToConfigInstanceCollection)this[""]; }
            set { this[""] = value; }
        }
    }

    public class MailToConfigInstanceCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new MailToConfigInstanceElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((MailToConfigInstanceElement)element).Address;
        }
    }

    public class MailToConfigInstanceElement : ConfigurationElement
    {
        [ConfigurationProperty("address", IsKey = true, IsRequired = true)]
        public string Address
        {
            get
            {
                return (string)base["address"];
            }
        }
    }

    public static class ConfigurationLoader
    {
        public static IList<string> GetMailToList()
        {
            var addresses = new List<string>();

            MailToConfigSection config = ConfigurationManager.GetSection("mailToList") as MailToConfigSection;

            if (config != null)
            {
                foreach (MailToConfigInstanceElement e in config.Instances)
                {
                    addresses.Add(e.Address);
                }
            }

            return addresses;
        }
    }
}
