using System;
using System.Threading;
using Microsoft.ConnectTheDots.Adapters;

namespace ConsoleApplication1
{
    class Program
    {
        public static void Main(string[] args)
        {
            SerialPortAdapter sp = new SerialPortAdapter();
            sp.Start("/dev/ttyAMA0", 115200);
            
            // work for 10 seconds and stop.
            Thread.Sleep( 10000 );
            sp.Stop();
        }
    }
}