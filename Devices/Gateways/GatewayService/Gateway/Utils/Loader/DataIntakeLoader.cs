using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using Gateway.DataIntake;
using Gateway.Utils.Logger;
using Gateway.DataIntake;

namespace Gateway.Utils
{
    public class DataIntakeLoader
    {
        private readonly IList<IDataIntake> _DataIntakes = new List<IDataIntake>( );

        private ILogger _Logger;

        public DataIntakeLoader( IList<String> sources, ILogger logger )
        {
            _Logger = new SafeLogger( logger );


            // enumerate all types with a IDataIntake interface
            var nameTypeDict = new Dictionary<string, Type>(); 
            foreach(string s in sources) 
            {
                Assembly ass = Assembly.LoadFrom( s );

                foreach( Type t in ass.GetExportedTypes( ) )
                {
                    //Get all classes implement IUserInterface
                    if( t.GetInterface( "IDataIntake", false ) != null )
                    {
                        nameTypeDict.Add( t.Name, t );//Add to Dictonary
                    }
                }
            }

            foreach( Type t in nameTypeDict.Values )
            {
                DataIntakeAbstract di = ( DataIntakeAbstract )Activator.CreateInstance( t, new object[] { _Logger } );

                _DataIntakes.Add( di );
            }
        }

        public IList<IDataIntake> Intakes
        {
            get
            {
                return _DataIntakes;
            }
        }

        public void StartAll( Func<string, int> enqueue, DataArrivalEventHandler onDataArrival = null )
        {
            foreach( DataIntakeAbstract dataIntake in _DataIntakes )
            {
                try
                {
                    if( onDataArrival != null )
                    {
                        dataIntake.OnDataArrival += onDataArrival;
                    }

                    dataIntake.Start( enqueue );
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
