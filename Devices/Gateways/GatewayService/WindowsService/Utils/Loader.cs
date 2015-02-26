using System;
using System.Collections.Generic;
using System.Configuration;
using Gateway.DataIntake;

namespace WindowsService.Utils
{
    internal class AMQPConfig
    {
        public string AMQPSAddress;
        public string EventHubName;
        public string EventHubMessageSubject;
        public string EventHubDeviceId;
        public string EventHubDeviceDisplayName;
    };

    internal static class Loader
    {
        internal static IList<String> GetSources() 
        {
            var dataIntakes = new List<String>( );

            DataIntakeConfigSection config = ConfigurationManager.GetSection( "dataIntakes" ) as DataIntakeConfigSection;

            if( config != null )
            {
                foreach( DataIntakeConfigInstanceElement e in config.Instances )
                {
                    dataIntakes.Add( e.AssemblyPath );
                }
            }

            return dataIntakes;
        }

        internal static IList<SensorEndpoint> GetEndpoints()
        {
            var sensorEndpoints = new List<SensorEndpoint>();

            CoreTest.Utils.Loader.SensorEndpointConfigSection sensorEndpointItems = ConfigurationManager.GetSection("sensorEndpoints")
                as CoreTest.Utils.Loader.SensorEndpointConfigSection;

            if (sensorEndpointItems != null)
            {
                foreach (CoreTest.Utils.Loader.SensorEndpointConfigInstanceElement sensorEndpointItem in sensorEndpointItems.Instances)
                {
                    sensorEndpoints.Add(new SensorEndpoint
                    {
                        Name = sensorEndpointItem.Name,
                        Host = sensorEndpointItem.Host,
                        Port = sensorEndpointItem.Port,
                    });
                }
            }

            return sensorEndpoints;
        }

        internal static AMQPConfig GetAMQPConfig()
        {
            AMQPServiceConfigSection section = ConfigurationManager.GetSection("AMQPServiceConfig") as AMQPServiceConfigSection;
            AMQPConfig configData = null;

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

    public class DataIntakeConfigSection : ConfigurationSection
    {
        [ConfigurationProperty( "", IsRequired = true, IsDefaultCollection = true )]
        public DataIntakeConfigInstanceCollection Instances
        {
            get { return ( DataIntakeConfigInstanceCollection )this[ "" ]; }
            set { this[ "" ] = value; }
        }
    }

    public class DataIntakeConfigInstanceCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement( )
        {
            return new DataIntakeConfigInstanceElement( );
        }

        protected override object GetElementKey( ConfigurationElement element )
        {
            return ( ( DataIntakeConfigInstanceElement )element ).Name;
        }
    }

    public class DataIntakeConfigInstanceElement : ConfigurationElement
    {
        [ConfigurationProperty( "name", IsKey = true, IsRequired = true )]
        public string Name
        {
            get
            {
                return ( string )base[ "name" ];
            }
        }

        [ConfigurationProperty( "type", IsRequired = true )]
        public string TypeName
        {
            get
            {
                return ( string )base[ "type" ];
            }
        }

        [ConfigurationProperty( "assemblyPath", IsRequired = true )]
        public string AssemblyPath
        {
            get
            {
                return ( string )base[ "assemblyPath" ];
            }
        }
    }

    public class SensorEndpointConfigSection : ConfigurationSection
    {
        [ConfigurationProperty( "", IsRequired = true, IsDefaultCollection = true )]
        public SensorEndpointConfigInstanceCollection Instances
        {
            get { return ( SensorEndpointConfigInstanceCollection )this[ "" ]; }
            set { this[ "" ] = value; }
        }
    }

    public class SensorEndpointConfigInstanceCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement( )
        {
            return new SensorEndpointConfigInstanceElement( );
        }

        protected override object GetElementKey( ConfigurationElement element )
        {
            return ( ( SensorEndpointConfigInstanceElement )element ).Name;
        }
    }

    public class SensorEndpointConfigInstanceElement : ConfigurationElement
    {
        [ConfigurationProperty( "name", IsKey = true, IsRequired = true )]
        public string Name
        {
            get
            {
                return ( string )base[ "name" ];
            }
        }

        [ConfigurationProperty( "port", IsRequired = true )]
        public int Port
        {
            get
            {
                return ( int )base[ "port" ];
            }
        }

        [ConfigurationProperty( "host", IsRequired = true )]
        public string Host
        {
            get
            {
                return ( string )base[ "host" ];
            }
        }
    }
}