namespace ServiceMonitor
{
    using System;
    using System.Configuration;
    using Microsoft.ConnectTheDots.Common;

    //--//

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
