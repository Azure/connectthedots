using System;
using System.Threading;

namespace Test
{
    public class TestRunner
    {
        static void Main(string[] args)
        {

            WebServiceTest t1 = new WebServiceTest("http://localhost:8000/GatewayService/API/Enqueue?tagId=40&dataIntegrity=42&value=30.5&dataSourceName=test");
            t1.Run();
            Console.WriteLine(String.Format("Test completed, {0} messages sent", t1.TotalMessagesSent));

            CoreTest t2 = new CoreTest();
            t2.Run();
            Console.WriteLine(String.Format("Test completed, {0} messages sent", t2.TotalMessagesSent));

            // wait for logging tasks to complete
            Console.WriteLine("Press any key to exit");
            Console.ReadLine();
        }
    }
}
