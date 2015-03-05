namespace Microsoft.ConnectTheDots.GatewayService
{
    using System.ComponentModel;
    using System.Diagnostics;
    using Gateway;
    using Microsoft.ConnectTheDots.Common;
    using NLog;

    //--//

    public class EventLogger : ILogger
    {
        #region Singleton implementation

        private static EventLogger _eventLogger;
        private static readonly object _syncRoot = new object( );

        internal static EventLogger Instance
        {
            get
            {
                if( _eventLogger == null )
                {
                    lock( _syncRoot )
                    {
                        if( _eventLogger == null )
                        {
                            _eventLogger = new EventLogger( );
                        }
                    }
                }

                return _eventLogger;
            }
        }

        private static EventLog    _eventLog;
        private static NLog.Logger _NLog;

        private EventLogger( )
        {
            _NLog = LogManager.GetCurrentClassLogger( );
            _eventLog = new EventLog
            {
                Source = Constants.WindowsServiceName,
                Log = "Application"
            };

            ( ( ISupportInitialize )( _eventLog ) ).BeginInit( );
            if( !EventLog.SourceExists( _eventLog.Source ) )
            {
                EventLog.CreateEventSource( _eventLog.Source, _eventLog.Log );
            }
            ( ( ISupportInitialize )( _eventLog ) ).EndInit( );
        }

        #endregion

        public void LogError( string logMessage )
        {
            _NLog.Error( logMessage );
            _eventLog.WriteEntry( logMessage, EventLogEntryType.Error );
        }

        public void LogInfo( string logMessage )
        {
            _NLog.Info( logMessage );
            _eventLog.WriteEntry( logMessage, EventLogEntryType.Information );
        }
    }
}
