namespace Gateway.Utils.Logger
{
    public interface ILogger
    {
        void LogError(string logMessage);

        void LogInfo(string logMessage);
    }
}
