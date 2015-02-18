using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using Gateway.DataIntake;
using Gateway.Utils.Logger;

namespace CoreTest.Utils.Loader
{
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