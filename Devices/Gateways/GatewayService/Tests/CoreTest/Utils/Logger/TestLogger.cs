using System.Diagnostics;
using SharedInterfaces;

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
