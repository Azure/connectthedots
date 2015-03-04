using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;
using System.Diagnostics;
using System.Collections.Specialized;
using SharedInterfaces;
using Gateway.Utils.Logger;


namespace ServiceMonitor
{
    class Program
    {
        private static ILogger _logger = SafeLogger.FromLogger( MonitorLogger.Instance );

        //--//

        private static Process _target;

        //--//
        
        static void Main( string[] args )
        {
            // try and open the GatewayService process
            string monitoringTarget = ConfigurationManager.AppSettings.Get( "MonitoringTarget" );
            string monitoringExecutable = ConfigurationManager.AppSettings.Get( "TargetExecutable" );

            if( String.IsNullOrEmpty( monitoringTarget ) || String.IsNullOrEmpty( monitoringExecutable ) )
            {
                _logger.LogError( "Error in configuration, cannot start monitoring" ); 
            }
            if(!File.Exists(monitoringExecutable)) 
            {
                _logger.LogInfo( "Executable does not exists in the current directory" ); 
            }

            Process target = null;

            //
            // try and open the monitored process
            //
            Process[] processes = Process.GetProcessesByName( monitoringTarget );

            if(processes.Length == 1) 
            {
                target = processes[0];
            }
            if( processes.Length > 1 || processes.Length == 0)
            {
                // if there is more than 1, kill them all and restart
                foreach( Process p in processes )
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
                target = CreateProcess( monitoringExecutable );                
            }

            if( target != null )
            {
                _target = target;

                // monitoring loop
                while( true )
                {
                    Thread.Sleep( 1000 );

                    if( target != null && target.HasExited )
                    {
                        _logger.LogInfo( String.Format( "Process '{0}' exited at time {1} with code {2}", monitoringExecutable, target.ExitTime, target.ExitCode ) );

                        target = _target;
                    }
                }
            }
        }

        private static Process CreateProcess( string monitoringExecutable  )
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

        private static void Restart( object sender, EventArgs e)
        {
            _target = CreateProcess( ( ( Process )sender ).StartInfo.FileName ); 
        }
    }
}
