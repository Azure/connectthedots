using System.Diagnostics;
using Gateway.Utils.Logger;

namespace CoreTest.Utils.Logger
{
    public class TestLogger : ILogger
    {
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
