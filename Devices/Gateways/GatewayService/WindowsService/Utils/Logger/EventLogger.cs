using System.ComponentModel;
using System.Diagnostics;
using Gateway;
using SharedInterfaces;

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

        private EventLogger() 
        {
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
        }

		#endregion

        private static EventLog _EventLog;

        public void LogError(string logMessage)
        {
            _EventLog.WriteEntry(logMessage, EventLogEntryType.Error);
        }

        public void LogInfo(string logMessage)
        {
            _EventLog.WriteEntry(logMessage, EventLogEntryType.Information);
        }
    }
}
