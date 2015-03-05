namespace Microsoft.ConnectTheDots.Test
{
    using System;
    using Microsoft.ConnectTheDots.Common;

    //--//

    public class ConsoleLogger : ILogger
    {
        public void LogError( string logMessage )
        {
            Console.Out.WriteLine( "[ERROR]: " + logMessage );
        }

        public void LogInfo( string logMessage )
        {
            Console.Out.WriteLine( "[INFO ] : " + logMessage );
        }
    }
}
