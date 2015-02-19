using System;
using Gateway.Models;
using CoreTest.Utils.Generators;
using Newtonsoft.Json;

namespace CoreTest
{
    public class TestRunner
    {
        static void Main(string[] args)
        {
            /////////////////////////////////////////////////////////////////////////////////////////////
            // Test core service 
            //
            CoreTest t2 = new CoreTest( );
            t2.Run( );
            Console.WriteLine( String.Format( "Core Test completed" ) );

            //////////////////////////////////////////////////////////////////////////////////////////////
            // Test Web service and core service 
            //
            SensorDataContract sensorData = RandomSensorDataGenerator.Generate( );
            string serializedData = JsonConvert.SerializeObject( sensorData );

            WebServiceTest t1 = new WebServiceTest( "http://localhost:8000/GatewayService/API/Enqueue?jsonData=" + serializedData );
            t1.Run( );
            Console.WriteLine(String.Format("WebService Test completed, {0} messages sent", t1.TotalMessagesSent));

            /////////////////////////////////////////////////////////////////////////////////////////////
            // Test Socket
            //
            SocketTest t3 = new SocketTest();
            t3.Run();
            Console.WriteLine(String.Format("Socket Test completed"));

            // wait for logging tasks to complete
            Console.WriteLine( "Press enter to exit" );
            Console.ReadLine( );
        }
    }
}
