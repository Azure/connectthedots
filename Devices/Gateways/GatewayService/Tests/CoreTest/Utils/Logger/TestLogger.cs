namespace Microsoft.ConnectTheDots.Test
{
    using System.Diagnostics;
    using Microsoft.ConnectTheDots.Common;

    //--//

    public class TestLogger : ILogger
    {
        #region Singleton implementation

        private static readonly object     _syncRoot = new object( );
        private static          TestLogger _logger;

        internal static TestLogger Instance
        {
            get
            {
                if( _logger == null )
                {
                    lock( _syncRoot )
                    {
                        if( _logger == null )
                        {
                            _logger = new TestLogger( );
                        }
                    }
                }

                return _logger;
            }
        }

        private TestLogger( )
        {
        }

        #endregion

        public void LogError( string logMessage )
        {
            Debug.WriteLine( "[ERROR]: " + logMessage );
        }

        public void LogInfo( string logMessage )
        {
            Debug.WriteLine( "[INFO ] : " + logMessage );
        }
    }
}
