namespace ServiceMonitor
{
    using System.ComponentModel;
    using System.Diagnostics;
    using NLog;
    using Microsoft.ConnectTheDots.Common;

    //--//

    public class MonitorLogger : ILogger
    {
        #region Singleton implementation

        private static readonly object          _SyncRoot = new object( );
        private static          MonitorLogger   _logger;
        private static          NLog.Logger     _NLog;

        //--//

        internal static MonitorLogger Instance
        {
            get
            {
                if( _logger == null )
                {
                    lock( _SyncRoot )
                    {
                        if( _logger == null )
                        {
                            _logger = new MonitorLogger( );
                        }
                    }
                }

                return _logger;
            }
        }

        private MonitorLogger( )
        {
            _NLog = LogManager.GetCurrentClassLogger( );
        }

        #endregion

        public void LogError( string logMessage )
        {
            _NLog.Error( logMessage );
        }

        public void LogInfo( string logMessage )
        {
            _NLog.Info( logMessage );
        }
    }
}
