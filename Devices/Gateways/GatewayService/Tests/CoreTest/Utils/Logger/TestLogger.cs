namespace Microsoft.ConnectTheDots.Test
{
    using System.Diagnostics;
    using Microsoft.ConnectTheDots.Common;

    //--//

    public class TestLogger : ILogger
    {
        #region Singleton implementation

        private static TestLogger _testLogger;
        private static readonly object _SyncRoot = new object();

        internal static TestLogger Instance
        {
            get
            {
                if (_testLogger == null)
                {
                    lock (_SyncRoot)
                    {
                        if (_testLogger == null)
                        {
                            _testLogger = new TestLogger( );
                        }
                    }
                }

                return _testLogger;
            }
        }

        private TestLogger()
        {
        }

		#endregion

        public void LogError(string logMessage)
        {
            Debug.WriteLine("[ERROR]: " + logMessage);
        }

        public void LogInfo(string logMessage)
        {
            Debug.WriteLine("[INFO ] : " + logMessage);
        }
    }
}
