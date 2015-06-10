//  ---------------------------------------------------------------------------------
//  Copyright (c) Microsoft Open Technologies, Inc.  All rights reserved.
// 
//  The MIT License (MIT)
// 
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
// 
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
// 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
//  ---------------------------------------------------------------------------------

namespace Microsoft.ConnectTheDots.Adapters
{
    using System;
    using System.IO.Ports;
    using System.Threading;
    using Microsoft.ConnectTheDots.Common;
    using Microsoft.ConnectTheDots.Common.Threading;
    using Microsoft.ConnectTheDots.Gateway;

    //--//

    public class BluetoothUARTAdapter : DeviceAdapterAbstract
    {
        private Func<string, int>    _enqueue;
        private bool                 _doWorkSwitch;

        //--//

        private const string         PORT           = "/dev/ttyAMA0";
        private const int            BAUD_RATE      = 9600;

        //--//

        public BluetoothUARTAdapter( ILogger logger )
            : base( logger )
        {
            _doWorkSwitch = true;
        }

        public override bool Start( Func<string, int> enqueue )
        {
            _enqueue = enqueue;

            _doWorkSwitch = true;

            var sh = new SafeAction( ( ) => Listen( PORT, BAUD_RATE ), _logger );

            TaskWrapper.Run( sh.SafeInvoke );

            return true;
        }

        public override bool Stop( )
        {
            _doWorkSwitch = false;

            return true;
        }

        public override bool SetEndpoint( SensorEndpoint endpoint = null )
        {
            //we don't need any endpoints for this Data Intake
            if( endpoint == null )
                return true;

            return false;
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
                            
                            // Send JSON message to the Cloud
                            _enqueue( valuesJson );
                        }
                        catch( Exception e )
                        {
                            _logger.LogError( "Error Reading from Serial Portand sending data from serial port " + serialPortName + ":" + e.Message );
                            serialPort.Close( );
                            serialPortAlive = false;
                        }
#endif
                    } while( serialPortAlive );

                }
                catch( Exception e )
                {
                    _logger.LogError( "Error processing data from serial port: " + e.Message );
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
                    _logger.LogError( "Error when trying to close the serial port: " + e.Message );
                }
                // We restart the thread if there has been some failure when reading from serial port
                Thread.Sleep( 800 );
            }
        }
    }
}