namespace ServiceMonitor
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.ServiceProcess;
    using System.Threading;
    using Microsoft.ConnectTheDots.Common;

    //--//

    internal class ServiceMonitor : IMonitor
    {
        protected const int MONITORING_INTERVAL = 1000; // ms

        //--//

        private string            _serviceName;
        private ServiceController _target;
        private bool              _exit;

        //--//

        protected ILogger _logger;

        //--//

        public ServiceMonitor( string serviceName, ILogger logger )
        {
            _serviceName = serviceName;
            _logger = SafeLogger.FromLogger( logger );

            // check that the file we are supposed to launch actually exists 
            Directory.SetCurrentDirectory( Path.GetDirectoryName( Assembly.GetExecutingAssembly( ).Location ) );

            ServiceController[] svcs = ServiceController.GetServices( );

            foreach(ServiceController svc in svcs)
            {
                if(svc.DisplayName == serviceName)
                {
                    _target = svc;
                }
            }

            if( _target == null ) 
            {
                _logger.LogInfo( String.Format( "Service '{0}' is not installed", serviceName ) ); 
            }

            _exit = false;
        }

        public bool Lock( string monitoringTarget )
        {
            if(_target == null)
            {
                return false;
            }

            if( _target.Status == ServiceControllerStatus.Stopped || _target.Status == ServiceControllerStatus.StopPending )
            {
                // create new process
                _target.Start( );
            }

            return true;
        }

        public void Monitor( )
        {
            // monitoring loop
            while( _exit == false )
            {
                Thread.Sleep( MONITORING_INTERVAL );

                if( _target != null )
                {
                    _target.Refresh( );

                    if( _target.Status == ServiceControllerStatus.Stopped )
                    {
                        _logger.LogInfo( String.Format( "Service '{0}' stopped at time {1} or earlier", _serviceName, DateTime.Now.ToString( ) ) );

                        Restart( );
                    }
                }
            }
        }

        public void QuitMonitor( )
        {
            _exit = true;
        }


        private void Restart( )
        {
            _target.Start();
        }
    }
}
