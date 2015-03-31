namespace Microsoft.ConnectTheDots.Adapters
{
    using System;
    using System.Collections.Generic;
    using System.IO.Ports;
    using System.Threading;
    using System.Threading.Tasks;

    //--//

    public class SerialPortAdapter
    {
        //--//
        
        private          bool                            _doWorkSwitch;

        //--//

        public SerialPortAdapter()
        {
            _doWorkSwitch = true;
        }
        
        public void Start( string port, int baudRate )
        {
            Task.Run(() => { Listen(port, baudRate); });
        }
        
        public void Stop( )
        {
            _doWorkSwitch = false;
        }

        private void Listen( string port, int baudRate )
        {
            string serialPortName = port;
            SerialPort serialPort = null;
            bool serialPortAlive = true;
            
            // We want the thread to restart listening on the serial port if it crashed
            while( _doWorkSwitch )
            {
                try
                {
#if !SIMULATEDATA
                    serialPort = new SerialPort( serialPortName, baudRate, Parity.None, 8, StopBits.One );
                    serialPort.DtrEnable = true;
                    serialPort.Open( );
#endif
                    do
                    {
                        // When simulating data, we will generate random data
                        // when not simulating, we read the serial port
                        string valuesJson = "";
#if !SIMULATEDATA
                        try
                        {
                            valuesJson = serialPort.ReadLine( );
                            
                            // do something with received data here.
                            Console.WriteLine(valuesJson);
                        }
                        catch( Exception e )
                        {
                            //_logger.LogError( "Error Reading from Serial Portand sending data from serial port " + serialPortName + ":" + e.Message );
                            Console.WriteLine( "Error Reading from Serial Portand sending data from serial port " + serialPortName + ":" + e.Message );
                            serialPort.Close( );
                            serialPortAlive = false;
                        }
#endif
                    } while( serialPortAlive );

                }
                catch( Exception e )
                {
                    // _logger.LogError( "Error processing data from serial port: " + e.Message );
                    Console.Write( "Error processing data from serial port: " + e.Message );
                }

                // When we are reaching this point, that means whether the COM port reading failed or the sensors has been disconnected
                // we will try to close the port properly, but if the device has been disconnected, this will trigger an exception
                try
                {
                    if( serialPort != null )
                    {
                        if( serialPort.IsOpen )
                        {
                            serialPort.Close( );
                        }

                        serialPort = null;
                    }
                }
                catch( Exception e )
                {
                    //_logger.LogError( "Error when trying to close the serial port: " + e.Message );
                    Console.WriteLine( "Error when trying to close the serial port: " + e.Message );
                }
                // We restart the thread if there has been some failure when reading from serial port
                Thread.Sleep( 800 );
            }
        }
    }
}