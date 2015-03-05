namespace Microsoft.ConnectTheDots.Common
{

    public class SafeLogger : ILogger
    {
        protected readonly ILogger _logger;

        //--//

        protected SafeLogger( ILogger logger )
        {
            _logger = logger;
        }

        static public SafeLogger FromLogger( ILogger logger )
        {
            if( logger is SafeLogger )
            {
                return ( SafeLogger )logger;
            }

            return new SafeLogger( logger );
        }

        public void LogError( string logMessage )
        {
            if( _logger != null )
            {
                _logger.LogError( logMessage );
            }
        }

        public void LogInfo( string logMessage )
        {
            if( _logger != null )
            {
                _logger.LogInfo( logMessage );
            }
        }
    }
}
