namespace WorkerHost
{
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;

    using Microsoft.ConnectTheDots.Common;
    using NLog;

    //--//

    public class WindowsEventLogger : ILogger
    {
        #region Singleton implementation

        private static readonly object _syncRoot = new object();

        //--//

        private static ILogger _WindowsEventLoggerInstance;
        private static EventLog _eventLog;

        //--//

        public static ILogger Instance
        {
            get
            {
                if (_WindowsEventLoggerInstance == null)
                {
                    lock (_syncRoot)
                    {
                        if (_WindowsEventLoggerInstance == null)
                        {
                            _WindowsEventLoggerInstance = new WindowsEventLogger();
                        }
                    }
                }

                return _WindowsEventLoggerInstance;
            }
        }

        private WindowsEventLogger()
        {
            _eventLog = new EventLog
            {
                Source = "TestWerker",
                Log = "Application"
            };

            ((ISupportInitialize)(_eventLog)).BeginInit();
            if (!EventLog.SourceExists(_eventLog.Source))
            {
                EventLog.CreateEventSource(_eventLog.Source, _eventLog.Log);
            }
            ((ISupportInitialize)(_eventLog)).EndInit();
        }

        #endregion

        public void Flush()
        {
        }

        public void LogError(string logMessage)
        {
            _eventLog.WriteEntry(logMessage, EventLogEntryType.Error);
        }

        public void LogInfo(string logMessage)
        {
            _eventLog.WriteEntry(logMessage, EventLogEntryType.Information);
        }
    }

    public class NLogEventLogger : ILogger
    {
        #region Singleton implementation

        private static readonly object _syncRoot = new object();

        //--//

        private static ILogger _NLogEventLoggerInstance;
        private static NLog.Logger _NLog;

        //--//

        public static ILogger Instance
        {
            get
            {
                if (_NLogEventLoggerInstance == null)
                {
                    lock (_syncRoot)
                    {
                        if (_NLogEventLoggerInstance == null)
                        {
                            _NLogEventLoggerInstance = new NLogEventLogger();
                        }
                    }
                }

                return _NLogEventLoggerInstance;
            }
        }

        private NLogEventLogger()
        {
            _NLog = LogManager.GetCurrentClassLogger();
            Directory.CreateDirectory("logs");
        }

        #endregion

        public void Flush()
        {
            LogManager.Flush();
        }

        public void LogError(string logMessage)
        {
            _NLog.Error(logMessage);
        }

        public void LogInfo(string logMessage)
        {
            _NLog.Info(logMessage);
        }
    }

    public static class EventLogger
    {
        private static readonly object _syncRoot = new object();

        //--//

        private static ILogger _eventLogger;

        //--//

        public static ILogger Instance
        {
            get
            {
                if (_eventLogger == null)
                {
                    lock (_syncRoot)
                    {
                        if (_eventLogger == null)
                        {
                            _eventLogger = NLogEventLogger.Instance;//Platform.IsMono ? NLogEventLogger.Instance : WindowsEventLogger.Instance;
                        }
                    }
                }

                return _eventLogger;
            }
        }
    }
}
