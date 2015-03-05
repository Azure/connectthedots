namespace Microsoft.ConnectTheDots.Common
{
    using System;

    //--//

    public class TunableLogger : ILogger
    {
        public enum LoggingLevel
        {
            Disabled = 0,
            Errors = 1,
            Verbose = 2,
            Undefined = 3,
        }

        //--//

        private LoggingLevel _level;

        //--//

        protected ILogger _Logger;

        //--//

        protected TunableLogger( ILogger logger )
        {
            _Logger = logger;
            _level = LoggingLevel.Errors;
        }

        //--//

        public static LoggingLevel LevelFromString( string value )
        {
            if( !String.IsNullOrEmpty( value ) )
            {
                if( value == LoggingLevel.Disabled.ToString( ) )
                {
                    return LoggingLevel.Disabled;
                }
                if( value == LoggingLevel.Errors.ToString( ) )
                {
                    return LoggingLevel.Errors;
                }
                if( value == LoggingLevel.Verbose.ToString( ) )
                {
                    return LoggingLevel.Verbose;
                }
            }

            return LoggingLevel.Undefined;
        }

        public static TunableLogger FromLogger( ILogger logger )
        {
            if( logger is TunableLogger )
            {
                return ( TunableLogger )logger;
            }

            return new TunableLogger( logger );
        }

        //--//

        public void LogError( string logMessage )
        {
            if( _level >= LoggingLevel.Errors )
            {
                _Logger.LogError( logMessage );
            }
        }

        public void LogInfo( string logMessage )
        {
            if( _level >= LoggingLevel.Verbose )
            {
                _Logger.LogInfo( logMessage );
            }
        }

        public LoggingLevel Level
        {
            get
            {
                return _level;
            }
            set
            {
                _level = value;
            }
        }
    }
}
