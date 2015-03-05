namespace Microsoft.ConnectTheDots.Test
{
    using System;
    using System.Configuration;
    using Newtonsoft.Json;
    using Microsoft.ConnectTheDots.Common;
    using Microsoft.ConnectTheDots.Gateway;

    //--//

    public class TestRunner
    {
        static void Main( string[] args )
        {
            // we do not need a tunable logger, but this is a nice way to test it...
            TunableLogger logger = TunableLogger.FromLogger(
                    SafeLogger.FromLogger( TestLogger.Instance )
                    );

            TunableLogger.LoggingLevel loggingLevel = TunableLogger.LevelFromString( ConfigurationManager.AppSettings.Get( "LoggingLevel" ) );

            logger.Level = ( loggingLevel != TunableLogger.LoggingLevel.Undefined ) ? loggingLevel : TunableLogger.LoggingLevel.Errors;


            /////////////////////////////////////////////////////////////////////////////////////////////
            // Test core service 
            //
            CoreTest t2 = new CoreTest( logger );
            t2.Run( );
            Console.WriteLine( String.Format( "Core Test completed" ) );

            //////////////////////////////////////////////////////////////////////////////////////////////
            // Test Web service and core service 
            //
            SensorDataContract sensorData = RandomSensorDataGenerator.Generate( );
            string serializedData = JsonConvert.SerializeObject( sensorData );

            WebServiceTest t1 = new WebServiceTest( "http://localhost:8000/GatewayService/API/Enqueue?jsonData=" + serializedData, logger );
            t1.Run( );
            Console.WriteLine( String.Format( "WebService Test completed, {0} messages sent", t1.TotalMessagesSent ) );

            /////////////////////////////////////////////////////////////////////////////////////////////
            // Test Socket
            //
            SocketTest t3 = new SocketTest( logger );
            t3.Run( );
            Console.WriteLine( String.Format( "Socket Test completed" ) );

            // wait for logging tasks to complete
            Console.WriteLine( "Press enter to exit" );
            Console.ReadLine( );
        }
    }
}
