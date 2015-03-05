namespace Microsoft.ConnectTheDots.Common
{
    public interface ILogger
    {
        void LogError( string logMessage );

        void LogInfo( string logMessage );
    }
}
