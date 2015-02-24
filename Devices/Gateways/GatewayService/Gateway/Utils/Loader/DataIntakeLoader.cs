using System;
using System.Collections.Generic;
using System.Reflection;
using Gateway.DataIntake;
using Gateway.Utils.Logger;
using SharedInterfaces;
using System.IO;
using System.Diagnostics;

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

            if ( sources.Count == 0 )
            {
                return;
            }

            //
            // enumerate all types with a IDataIntake interface look in the current directory, in the 
            // running assembly directory and in the entry and executing assembly
            //
            var directories = new List<String>( );
            directories.Add( Directory.GetCurrentDirectory( ) );
            var di1 = Path.GetDirectoryName( Assembly.GetEntryAssembly( ).Location ); 
            var di2 = Path.GetDirectoryName( Assembly.GetCallingAssembly( ).Location );
            var di3 = Path.GetDirectoryName( Assembly.GetExecutingAssembly( ).Location );

            // do not dupiclate search paths 
            if ( !directories.Contains( di1 ) )
            {
                directories.Add( di1 );
            }
            if ( !directories.Contains( di2 ) )
            {
                directories.Add( di2);
            }
            if ( !directories.Contains( di3 ) )
            {
                directories.Add( di3 );
            }

            // try and load assemblies from path 
            var nameTypeDict = new Dictionary<string, Type>( );
            foreach ( string s in sources )
            {
                try
                {
                    Assembly assm = null;
                    // try path from config first
                    if ( File.Exists( s ) )
                    {
                        assm = Assembly.LoadFrom( s );

                        // remember this directory as a potential source
                        var lastDir = Path.GetDirectoryName( s );
                        if ( !directories.Contains( lastDir ) )
                        {
                            directories.Add( lastDir ); 
                        }
                    }
                    else
                    {
                        // try the other directories
                        foreach(var d in directories)
                        {
                            var assmName =  Path.GetFileName( s ); 

                            // try again
                            string assmPath = Path.Combine( d, assmName );
                            if ( File.Exists( assmPath ) )
                            {
                                assm = Assembly.LoadFrom( s );
                            }
                        }

                        if(assm == null)
                        {
                            // Log that we did not load our data sources correctly
                            string allSources = "";
                            foreach(var src in sources)
                            {
                                allSources += s + ";";
                            }

                            _Logger.LogError( String.Format( "Following data intakes were specificied, but could not be loaded: {0}", allSources ) );  
                        }
                    }

                    Debug.Assert( assm != null);

                    foreach ( Type t in assm.GetExportedTypes( ) )
                    {
                        //Get all classes that implement the required interface
                        if ( t.GetInterface( "IDataIntake", false ) != null )
                        {
                            nameTypeDict.Add( t.Name, t ); //Add to Dictonary
                        }
                    }
                }
                catch ( Exception ex )
                {
                    //dont want to stop loading another modules if one fails
                    _Logger.LogError( String.Format( "Exception on loading DataIntake {0}: {1}", s, ex.Message ) );
                }
            }

            foreach( KeyValuePair<string, Type> t in nameTypeDict )
            {
                try
                {
                    DataIntakeAbstract di = (DataIntakeAbstract)Activator.CreateInstance(t.Value, new object[] { _Logger });

                    if( di != null )
                    {
                        _DataIntakes.Add(di);
                    }
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
        public void StartAll( Func<string, int> enqueue, DataArrivalEventHandler onDataArrival = null )
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

                    dataIntake.Start( OnDataToEnqueue );
                }
                catch( Exception ex )
                {
                    _Logger.LogError( ex.StackTrace );

                    // catch all other exceptions
                }
            }
        }
        public void StopAll( )
        {
            foreach( DataIntakeAbstract dataIntake in _DataIntakes )
            {
                try
                {
                    dataIntake.Stop();
                }
                catch( Exception ex )
                {
                    _Logger.LogError( ex.StackTrace );

                    // catch all exceptions
                }
            }
        }
    }
}
