using System;
using System.Threading;

namespace BatchSenderThreadTest
{
    class TestRunner
    {
        static void Main(string[] args)
        {
            BatchSenderThreadTest t = new BatchSenderThreadTest();
            t.Run();

            // wait for logging tasks to complete
            Console.WriteLine("Test completed, press any key to exit");
            Console.ReadLine();
        }
    }
}
