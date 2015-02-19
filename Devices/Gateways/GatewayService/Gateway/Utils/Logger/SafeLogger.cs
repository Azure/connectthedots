using SharedInterfaces;

namespace Gateway.Utils.Logger
{
    public class SafeLogger : ILogger
    {
        protected ILogger _Logger;

        public SafeLogger( ILogger logger )
        {
            _Logger = logger;
        }

        public void LogError( string logMessage )
        {
            if( _Logger != null )
            {
                _Logger.LogError( logMessage );
            }
        }

        public void LogInfo( string logMessage )
        {
            if( _Logger != null )
            {
                _Logger.LogInfo( logMessage );
            }
        }
    }
}
