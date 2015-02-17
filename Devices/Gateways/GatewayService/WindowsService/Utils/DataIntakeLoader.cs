using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using Gateway.DataIntake;
using Gateway.Utils.Logger;
using IDataIntake = Gateway.DataIntake.IDataIntake;

namespace WindowsService.Utils
{
    public class DataIntakeLoader
    {
        private static readonly List<IDataIntake> _DataIntakes = new List<IDataIntake>();
        private static readonly List<SensorEndpoint> _SensorEndpoints = new List<SensorEndpoint>();

        private static ILogger _Logger;
        public DataIntakeLoader(ILogger logger)
        {
            _Logger = logger;

            try
            {
                SensorEndpointConfigSection sensorEndpointItems = ConfigurationManager.GetSection("sensorEndpoints")
                 as SensorEndpointConfigSection;

                if (sensorEndpointItems != null)
                {
                    foreach (SensorEndpointConfigInstanceElement sensorEndpointItem in sensorEndpointItems.Instances)
                    {
                        _SensorEndpoints.Add(new SensorEndpoint
                        {
                            Host = sensorEndpointItem.Host,
                            Port = sensorEndpointItem.Port,
                        });
                    }
                }

                DataIntakeConfigSection config = ConfigurationManager.GetSection("dataIntakes")
                 as DataIntakeConfigSection;

                foreach (DataIntakeConfigInstanceElement e in config.Instances)
                {
                    try
                    {
                        logger.LogInfo("Loading Data Intake: " + e.TypeName + e.AssemblyPath);

                        Assembly ass = Assembly.LoadFrom(e.AssemblyPath);
                        Type handlerType = ass.GetType(e.TypeName);

                        IDataIntake dataIntake = (IDataIntake) Activator.CreateInstance(handlerType);
                        if(dataIntake.SetEndpoint())
                            _DataIntakes.Add(dataIntake);

                        foreach (SensorEndpoint sensorEndpoint in _SensorEndpoints)
                        {
                            IDataIntake dataIntakeWithEndpoint = (IDataIntake)Activator.CreateInstance(handlerType);
                            if (dataIntakeWithEndpoint.SetEndpoint(sensorEndpoint))
                                _DataIntakes.Add(dataIntakeWithEndpoint);
                        }
                    }
                    catch (Exception ex)
                    {
                        _Logger.LogError(ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex.Message);
            }
        }

        public void Start(Func<string, int> enqueue, ILogger loggerForDataIntake, Func<bool> doWorkSwitch)
        {
            try
            {
                foreach (IDataIntake dataIntake in _DataIntakes)
                {
                    dataIntake.Start(enqueue, loggerForDataIntake, doWorkSwitch);
                }
            }
            catch (StackOverflowException ex)
            {
                if (_Logger != null)
                {
                    _Logger.LogError(ex.Message);
                }

                // do not hide stack overflow exceptions
                throw;
            }
            catch (OutOfMemoryException ex)
            {
                if (_Logger != null)
                {
                    _Logger.LogError(ex.Message);
                }

                // do not hide memory exceptions
                throw;
            }
            catch (Exception ex)
            {
                if (_Logger != null)
                {
                    _Logger.LogError(ex.Message);
                }

                // catch all other exceptions
            }
        }
    }
    public class DataIntakeConfigSection : ConfigurationSection
    {
        [ConfigurationProperty("", IsRequired = true, IsDefaultCollection = true)]
        public DataIntakeConfigInstanceCollection Instances
        {
            get { return (DataIntakeConfigInstanceCollection)this[""]; }
            set { this[""] = value; }
        }
    }
    public class DataIntakeConfigInstanceCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new DataIntakeConfigInstanceElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((DataIntakeConfigInstanceElement)element).Name;
        }
    }

    public class DataIntakeConfigInstanceElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsKey = true, IsRequired = true)]
        public string Name
        {
            get
            {
                return (string)base["name"];
            }
        }

        [ConfigurationProperty("type", IsRequired = true)]
        public string TypeName
        {
            get
            {
                return (string)base["type"];
            }
        }

        [ConfigurationProperty("assemblyPath", IsRequired = true)]
        public string AssemblyPath
        {
            get
            {
                return (string)base["assemblyPath"];
            }
        }
    }

    public class SensorEndpointConfigSection : ConfigurationSection
    {
        [ConfigurationProperty("", IsRequired = true, IsDefaultCollection = true)]
        public SensorEndpointConfigInstanceCollection Instances
        {
            get { return (SensorEndpointConfigInstanceCollection)this[""]; }
            set { this[""] = value; }
        }
    }
    public class SensorEndpointConfigInstanceCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new SensorEndpointConfigInstanceElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((SensorEndpointConfigInstanceElement)element).Name;
        }
    }

    public class SensorEndpointConfigInstanceElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsKey = true, IsRequired = true)]
        public string Name
        {
            get
            {
                return (string)base["name"];
            }
        }

        [ConfigurationProperty("port", IsRequired = true)]
        public int Port
        {
            get
            {
                return (int)base["port"];
            }
        }

        [ConfigurationProperty("host", IsRequired = true)]
        public string Host
        {
            get
            {
                return (string)base["host"];
            }
        }
    }
 }