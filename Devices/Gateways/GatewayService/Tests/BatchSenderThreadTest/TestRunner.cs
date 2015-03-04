using System;
using System.Threading;
using CoreTest.Utils.Logger;
using Gateway.Utils.Logger;

namespace BatchSenderThreadTest
{
    class TestRunner
    {
        static void Main(string[] args)
        {
            BatchSenderThreadTest t = new BatchSenderThreadTest( SafeLogger.FromLogger( TestLogger.Instance ) );
            t.Run();

            // wait for logging tasks to complete
            Console.WriteLine("Test completed, press enter to exit");
            Console.ReadLine();
        }
    }
}
