using System.ComponentModel;
using System.Diagnostics;
using Gateway;
using SharedInterfaces;
using NLog;

namespace WindowsService.Utils.Logger
{
    public class EventLogger : ILogger
    {
        #region Singleton implementation

		      private static EventLogger _EventLogger;
        private static readonly object _SyncRoot = new object();

        public static EventLogger Instance
        {
            get
            {
                if (_EventLogger == null)
                {
                    lock (_SyncRoot)
                    {
                        if (_EventLogger == null)
                        {
                            _EventLogger = new EventLogger();
                        }
                    }
                }

                return _EventLogger;
            }
        }

        private static EventLog _EventLog;
        private static NLog.Logger _NLog;

        private EventLogger()
        {
            _NLog = LogManager.GetCurrentClassLogger();
            _EventLog = new EventLog
            {
                Source = Constants.WindowsServiceName,
                Log = "Application"
            };

            ((ISupportInitialize)(_EventLog)).BeginInit();
            if (!EventLog.SourceExists(_EventLog.Source))
            {
                EventLog.CreateEventSource(_EventLog.Source, _EventLog.Log);
            }
            ((ISupportInitialize)(_EventLog)).EndInit(); 

            LogInfo("Logger appeared.");
        }

		#endregion

        public void LogError(string logMessage)
        {
            _NLog.Error(logMessage);
            _EventLog.WriteEntry(logMessage, EventLogEntryType.Error);
        }

        public void LogInfo(string logMessage)
        {
            _NLog.Info(logMessage);
            _EventLog.WriteEntry(logMessage, EventLogEntryType.Information);
        }
    }
}
