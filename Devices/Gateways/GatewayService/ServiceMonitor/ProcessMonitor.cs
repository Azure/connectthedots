namespace ServiceMonitor
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Threading;
    using Microsoft.ConnectTheDots.Common;

    //--//

    internal class ProcessMonitor : IMonitor
    {
        protected const int MONITORING_INTERVAL = 1000; // ms

        //--//

        private  string _executableName;
        private Process _target;
        private bool    _exit;

        //--//

        protected ILogger _logger;

        //--//

        public ProcessMonitor( string executableName, ILogger logger )
        {
            _executableName = executableName;
            _logger = SafeLogger.FromLogger( logger );

            // cannot find executable, we may still be able to run based if such executable 
            // is in the path or already running
            Directory.SetCurrentDirectory( Path.GetDirectoryName( Assembly.GetExecutingAssembly( ).Location ) );

            if ( !File.Exists( _executableName ) )
            {
                // cannot find executable, we may still be able to run based on the target
                _logger.LogInfo( "Executable does not exists in the current directory" );
            }

            _exit = false;
        }

        public bool Lock( string monitoringTarget )
        {
            //
            // try and open the monitored process
            //
            Process[] processes = Process.GetProcessesByName( monitoringTarget );

            if ( processes.Length == 1 )
            {
                _target = processes[ 0 ];
            }

            if ( processes.Length > 1 || processes.Length == 0 )
            {
                // if there is more than 1, kill them all and restart
                foreach ( Process p in processes )
                {
                    _logger.LogInfo( String.Format( "Killing process {0}, PID: {1}", p.ProcessName, p.Id ) );

                    try
                    {
                        p.Kill( );
                    }
                    catch
                    {
                    }
                }

                // create new process
                _target = CreateProcess( _executableName );
            }

            return _target == null ? false : true;
        }

        public void Monitor( )
        {
            // monitoring loop
            while( _exit == false && _target != null )
            {
                Thread.Sleep( MONITORING_INTERVAL );

                if( _target.HasExited )
                {
                    _logger.LogInfo( String.Format( "Process '{0}' exited at time {1} with code {2}", _executableName, _target.ExitTime, _target.ExitCode ) );
                }
            }
        }

        public void QuitMonitor()
        {
            _exit = true;
        }

        private Process CreateProcess( string monitoringExecutable  )
        {
            Process p = new Process( );

            p.StartInfo = new ProcessStartInfo( monitoringExecutable );

            p.Exited += Restart;

            if(!p.Start( ))
            {
                _logger.LogError( String.Format( "Process '{0}'could not be started", monitoringExecutable ) );

                return null;
            }

            return p;
        }

        private void Restart( object sender, EventArgs e)
        {
            _target = CreateProcess( _executableName ); 
        }
    }
}
