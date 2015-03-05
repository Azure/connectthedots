namespace Microsoft.ConnectTheDots.Common
{

    public class SafeLogger : ILogger
    {
        protected ILogger _Logger;

        protected SafeLogger( ILogger logger )
        {
            _Logger = logger;
        }

        static public SafeLogger FromLogger( ILogger logger )
        {
            if(logger is SafeLogger)
            {
                return (SafeLogger)logger;
            }

            return new SafeLogger(logger); 
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
