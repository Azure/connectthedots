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
using System.Reflection;
using System.ComponentModel;


namespace ServiceMonitor
{
    class Program
    {
        private static ILogger _logger = SafeLogger.FromLogger( MonitorLogger.Instance );

        //--//

        static void Main( string[] args )
        {
            // try and open the GatewayService process
            string monitoringTarget = ConfigurationManager.AppSettings.Get( "MonitoringTarget" );
            string monitoringExecutable = ConfigurationManager.AppSettings.Get( "TargetExecutable" );

            if ( String.IsNullOrEmpty( monitoringTarget ) || String.IsNullOrEmpty( monitoringExecutable ) )
            {
                _logger.LogError( "Error in configuration, cannot start monitoring" );

                return;
            }

            IMonitor sm = new ServiceMonitor( monitoringTarget, _logger );
            if(sm.Lock( monitoringTarget ) )
            {
                sm.Monitor( );
            }

            IMonitor pm = new ProcessMonitor( monitoringExecutable, _logger );

            if(pm.Lock( monitoringTarget ))
            {
                // never returns unless IMonitor.QuitMonitor is called
                pm.Monitor( );
            }

        }

    }
}
