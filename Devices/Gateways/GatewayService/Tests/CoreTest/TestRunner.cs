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

            CoreTest t2 = new CoreTest();
            t2.Run();
            Console.WriteLine(String.Format("Test completed, {0} messages sent", t2.TotalMessagesSent));


            SensorDataContract sensorData = RandomSensorDataGenerator.Generate( );
            string serializedData = JsonConvert.SerializeObject( sensorData );

            WebServiceTest t1 = new WebServiceTest( "http://localhost:8000/GatewayService/API/Enqueue?jsonData=" + serializedData );
            t1.Run( );
            Console.WriteLine( String.Format( "Test completed, {0} messages sent", t1.TotalMessagesSent ) );


            // wait for logging tasks to complete
            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
        }
    }
}
