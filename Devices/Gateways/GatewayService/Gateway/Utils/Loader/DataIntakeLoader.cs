namespace Microsoft.ConnectTheDots.Gateway
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.IO;
    using System.Diagnostics;
    using Microsoft.ConnectTheDots.Common;

    //--//

    public class DataIntakeLoader
    {
        private readonly IList<DataIntakeAbstract> _DataIntakes = new List<DataIntakeAbstract>( );

        //dont want duplicated endpoints
        private static HashSet<SensorEndpoint> _SensorEndpoints = new HashSet<SensorEndpoint>( );

        private readonly ILogger _Logger;

        public DataIntakeLoader( IList<String> sources, IList<SensorEndpoint> endpoints, ILogger logger )
        {
            _Logger = SafeLogger.FromLogger( logger );

            _Logger.LogInfo( "Starting loading Data Intakes" );

            //for each filename will store a flag - whether it was specified at config or not
            Dictionary<String, bool> sourcesToLoad = new Dictionary<string, bool>( );
            if( sources != null )
            {
                foreach( var filename in sources )
                {
                    sourcesToLoad.Add( filename, true );
                }
            }
            else
            {
                _Logger.LogInfo( "No list of DataIntakes in configuration file, continuing..." );
            }

            if( endpoints != null )
            {
                foreach( SensorEndpoint endpoint in endpoints )
                {
                    _SensorEndpoints.Add( endpoint );
                }
            }
            else
            {
                _Logger.LogInfo( "No list of SensorEndpoints in configuration file, continuing..." );
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

            // do not duplicate search paths 
            if( !directories.Contains( di1 ) )
            {
                directories.Add( di1 );
            }
            if( !directories.Contains( di2 ) )
            {
                directories.Add( di2 );
            }
            if( !directories.Contains( di3 ) )
            {
                directories.Add( di3 );
            }

            foreach( string directory in directories )
            {
                //Dinar: dont want to try all windows/system32 directory for now
                if( !directory.ToLowerInvariant( ).Contains( "system32" ) )
                {
                    foreach( string filename in Directory.GetFiles( directory ) )
                    {
                        //false flag - file was not specified at config
                        sourcesToLoad.Add( filename, false );
                    }
                }
            }

            // try and load assemblies from path 
            var nameTypeDict = new Dictionary<string, Type>( );
            string notLoadedSpecifiedSources = "";
            string notLoadedNotSpecifiedSources = "";

            foreach( KeyValuePair<string, bool> source in sourcesToLoad )
            {
                string fileName = source.Key;
                try
                {
                    Assembly assm = null;
                    // try path from config first
                    if( File.Exists( fileName ) )
                    {
                        assm = Assembly.LoadFrom( fileName );

                        // remember this directory as a potential source
                        var lastDir = Path.GetDirectoryName( fileName );
                        if( !directories.Contains( lastDir ) )
                        {
                            directories.Add( lastDir );
                        }
                    }
                    else
                    {
                        // try the other directories
                        foreach( var d in directories )
                        {
                            var assmName = Path.GetFileName( fileName );

                            // try again
                            string assmPath = Path.Combine( d, assmName );
                            if( File.Exists( assmPath ) )
                            {
                                assm = Assembly.LoadFrom( fileName );
                            }
                        }

                        if( assm == null )
                        {
                            // Log that we did not load our data sources correctly
                            if( source.Value )
                            {
                                notLoadedSpecifiedSources += fileName;
                            }
                            else
                            {
                                notLoadedNotSpecifiedSources += fileName;
                            }
                            notLoadedSpecifiedSources += "; ";
                            continue;
                        }
                    }

                    Debug.Assert( assm != null );

                    foreach( Type t in assm.GetExportedTypes( ) )
                    {
                        //Get all classes that implement the required interface
                        if( t.GetInterface( "IDataIntake", false ) != null )
                        {
                            _Logger.LogInfo( "IDataIntake assembly loaded: " + t.Name );

                            nameTypeDict.Add( t.Name, t ); //Add to Dictonary
                        }
                    }
                }
                catch( Exception )
                {
                    //dont want to stop loading another modules if one fails
                    if( source.Value )
                    {
                        notLoadedSpecifiedSources += fileName;
                    }
                    else
                    {
                        notLoadedNotSpecifiedSources += fileName;
                    }
                    notLoadedNotSpecifiedSources += "; ";
                }
            }

            if( notLoadedSpecifiedSources.Length > 0 )
            {
                _Logger.LogError( String.Format( "Following Data Intakes were specificied, but could not be loaded: {0}", notLoadedSpecifiedSources ) );
            }
            if( notLoadedNotSpecifiedSources.Length > 0 )
            {
                _Logger.LogError( String.Format( "Following files are not specificied, and could not be loaded as Data Intakes: {0}", notLoadedNotSpecifiedSources ) );
            }

            foreach( KeyValuePair<string, Type> t in nameTypeDict )
            {
                try
                {
                    DataIntakeAbstract di = ( DataIntakeAbstract )Activator.CreateInstance( t.Value, new object[] { _Logger } );

                    if( di != null )
                    {
                        _Logger.LogInfo( "IDataIntake instance created: " + t.Key );

                        //adding instance without endpoint if acceptable
                        if( di.SetEndpoint( ) )
                        {
                            _DataIntakes.Add( di );
                        }

                        foreach( SensorEndpoint sensorEndpoint in _SensorEndpoints )
                        {
                            DataIntakeAbstract diWithEndpoint = ( DataIntakeAbstract )Activator.CreateInstance( t.Value, new object[] { _Logger } );
                            if( diWithEndpoint.SetEndpoint( sensorEndpoint ) )
                            {
                                _DataIntakes.Add( diWithEndpoint );
                            }
                        }
                    }
                }
                catch( Exception ex )
                {
                    // dont want to stop creating another instances if one fails
                    _Logger.LogError( String.Format( "Exception on Creating DataIntake Instance \"{0}\": {1}", t.Key, ex.Message ) );
                }
            }
        }

        public IList<IDataIntake> Intakes
        {
            get
            {
                return ( IList<IDataIntake> )_DataIntakes;
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
                    _Logger.LogError( "Exception on Starting DataIntake: " + ex.StackTrace );

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
                    dataIntake.Stop( );
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
