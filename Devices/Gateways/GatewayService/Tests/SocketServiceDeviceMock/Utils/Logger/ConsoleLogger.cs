using System;
using SharedInterfaces;

namespace SocketDeviceMock.Utils.Logger
{
    public class ConsoleLogger : ILogger
    {
        public void LogError(string logMessage)
        {
            Console.Out.WriteLine("[ERROR]: " + logMessage);
        }

        public void LogInfo(string logMessage)
        {
            Console.Out.WriteLine("[INFO ] : " + logMessage);
        }
    }
}
