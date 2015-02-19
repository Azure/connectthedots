using System;
using System.Collections.Generic;
using System.Reflection;
using Gateway.DataIntake;
using Gateway.Utils.Logger;
using SharedInterfaces;

namespace Gateway.Utils.Loader
{
    public class DataIntakeLoader
    {
        private readonly IList<IDataIntake> _DataIntakes = new List<IDataIntake>( );

        private readonly ILogger _Logger;

        public DataIntakeLoader( IList<String> sources, ILogger logger )
        {
            _Logger = new SafeLogger( logger );

            if (sources == null)
            {
                throw new ArgumentException("List of DataIntake sources could be empty but not null");
            }

            // enumerate all types with a IDataIntake interface
            var nameTypeDict = new Dictionary<string, Type>(); 
            foreach(string s in sources) 
            {
                try
                {
                    Assembly ass = Assembly.LoadFrom(s);

                    foreach (Type t in ass.GetExportedTypes())
                    {
                        //Get all classes that implement the required interface
                        if (t.GetInterface("IDataIntake", false) != null)
                        {
                            nameTypeDict.Add(t.Name, t); //Add to Dictonary
                        }
                    }
                }
                catch (Exception ex)
                {
                    //dont want to stop loading another modules if one fails
                    _Logger.LogError(String.Format("Exception on loading DataIntake {0}: {1}", s, ex.Message));
                }
            }

            foreach( KeyValuePair<string, Type> t in nameTypeDict )
            {
                try
                {
                    DataIntakeAbstract di = (DataIntakeAbstract)Activator.CreateInstance(t.Value, new object[] { _Logger });

                    if( di != null )
                        _DataIntakes.Add(di);
                }
                catch (Exception ex)
                {
                    // dont want to stop creating another instances if one fails
                    _Logger.LogError(String.Format("Exception on Creating Instance {0}: {1}", t.Key, ex.Message));
                }
            }
        }

        public IList<IDataIntake> Intakes
        {
            get
            {
                return _DataIntakes;
            }
        }

        protected Func<string, int> OnDataToEnqueue;
        public void StartAll(Func<string, int> enqueue, DataArrivalEventHandler onDataArrival = null)
        {
            foreach( DataIntakeAbstract dataIntake in _DataIntakes )
            {
                try
                {
                    if( onDataArrival != null )
                    {
                        OnDataToEnqueue =
                            data =>
                            {
                                onDataArrival( data );

                                return enqueue( data );
                            };
                    }
                    else
                    {
                        OnDataToEnqueue = enqueue;
                    }

                    dataIntake.Start(OnDataToEnqueue);
                }
                catch( Exception ex )
                {
                    _Logger.LogError( ex.StackTrace );

                    // catch all other exceptions
                }
            }
        }
    }
}
