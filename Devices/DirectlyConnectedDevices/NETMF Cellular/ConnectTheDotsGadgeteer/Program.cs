//  ---------------------------------------------------------------------------------
//  Copyright (c) Microsoft Open Technologies, Inc.  All rights reserved.
// 
//  The MIT License (MIT)
// 
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sub-license, and/or sell
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

using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Amqp;
using Amqp.Framing;
using Amqp.Types;
using Gadgeteer.Modules.GHIElectronics;
using GHI.Networking;
using Json.NETMF;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;

namespace ConnectTheDotsGadgeteer
{
    public partial class Program
    {

        #region Confidential Settings

        //private const string SSID = "YourSSID";

        //private const string WIFI_PASSWORD = "WifiPassword";

        private const string apn = "apn.zerogravitywireless.com";
        private const string username = "";
        private const string password = "";

        // Azure Event Hub connection information
        private const string AMQPAddress =
            "amqps://D1:%2FJAslH3L%2FA9LySC8%2B1CzUiSjyWtAnfxyvj2bZYd95DQ%3D@blakeyyc-ns.servicebus.windows.net";

        // unique GUID per device. Use GUIDGEN to generate new one
        private const string SensorGUID = "AC615ABE-3C0C-41C3-991F-B8A9009EE04D";

        #endregion

        // Azure Event Hub connection information
        private const string SensorName = "Cellular Gadgeteer"; // Name of the device you want to display
        private const string Location = "Could be anywhere"; // Location of the device
        private const string EventHub = "ehdevices"; // Azure event hub name
        private const string Organization = "ZingPow"; // Your organization name

        // Define the frequency at which we want the device to send its sensor data (in milliseconds)
        private const int SendFrequency = 10000;
        private readonly GT.Timer _timer = new GT.Timer(SendFrequency);

        // AMQP connection to Azure Event Hubs resources
        private Address _address;
        private Connection _connection;
        private SenderLink _sender;
        private Session _session;
        // Using InterlockExchange to ensure there is no concurrency in using the AMQP connexion
        private static int _isSendingMessage;

        // This method is run when the mainboard is powered up or reset.   
        private void ProgramStarted()
        {
            // Use Debug.Print to show messages in Visual Studio's "Output" window during debugging.
            Debug.Print("Program Started");

            _timer.Tick += _timer_Tick;

            //setup event handlers needed for cellular module for this project
            cellularRadio.NetworkDown += cellularRadio_NetworkDown;
            cellularRadio.NetworkUp += cellularRadio_NetworkUp;

            cellularRadio.GprsNetworkRegistrationChanged += cellularRadio_GprsNetworkRegistrationChanged;

            cellularRadio.DebugPrintEnabled = true;

            cellularRadio.LineSent += (s, e) => Debug.Print("Sent: " + e.TrimEnd('\r', '\n')); //Handy for debugging

            cellularRadio.LineReceived += (s, e) => Debug.Print("Recv: " + e.TrimEnd('\r', '\n')); //Handy for debugging

            cellularRadio.PowerOn();
        }

        private void cellularRadio_GprsNetworkRegistrationChanged(CellularRadio sender,
    CellularRadio.NetworkRegistrationState networkState)
        {
            switch (networkState)
            {
                case CellularRadio.NetworkRegistrationState.Error:
                    Debug.Print("Gprs Network Registration Changed - Error");
                    break;
                case CellularRadio.NetworkRegistrationState.NotSearching:
                    Debug.Print("Gprs Network Registration Changed - Not Searching");
                    break;
                case CellularRadio.NetworkRegistrationState.Registered:
                    Debug.Print("Gprs Network Registration Changed - Registered");
                    cellularRadio.UseThisNetworkInterface(apn, username, password, PPPSerialModem.AuthenticationType.Pap); //we have GPRS, so start up the PPP
                    break;
                case CellularRadio.NetworkRegistrationState.RegistrationDenied:
                    Debug.Print("Gprs Network Registration Changed - Registration Denied");
                    break;
                case CellularRadio.NetworkRegistrationState.Roaming:
                    Debug.Print("Gprs Network Registration Changed - Roaming");
                    break;
                case CellularRadio.NetworkRegistrationState.Searching:
                    Debug.Print("Gprs Network Registration Changed - Searching");
                    break;
                case CellularRadio.NetworkRegistrationState.Unknown:
                    Debug.Print("Gprs Network Registration Changed - Unknown");
                    break;
            }
        }

        private void cellularRadio_NetworkUp(GTM.Module.NetworkModule sender,
            GTM.Module.NetworkModule.NetworkState state)
        {
            switch (state)
            {
                case GTM.Module.NetworkModule.NetworkState.Down:
                    Debug.Print("Network Up Event - Down");
                    _timer.Stop();
                    break;
                case GTM.Module.NetworkModule.NetworkState.Up:
                    Debug.Print("Network Up Event - Up " + cellularRadio.NetworkInterface.IPAddress);

                    if (cellularRadio.NetworkInterface.IPAddress != "0.0.0.0")
                    {
                        //have a PPP connection and valid IP address
                        Debug.Print("IP Address: " + cellularRadio.NetworkInterface.IPAddress);
                        Debug.Print("Gateway Address: " + cellularRadio.NetworkInterface.GatewayAddress);
                        foreach (var dns in cellularRadio.NetworkInterface.DnsAddresses)
                        {
                            Debug.Print("DNS Address: " + dns);
                        }

                        // get Internet Time using NTP
                        NTPTime("time.windows.com", -360);
                        Debug.Print("Time: " + DateTime.Now);

                        // Initialize AMQP _connection with Azure Event Hub
                        InitAMQPconnection();

                        //start collecting and sending data to Azure.
                        _timer.Start();
                    }
                    break;
            }
        }

        private void cellularRadio_NetworkDown(GTM.Module.NetworkModule sender,
            GTM.Module.NetworkModule.NetworkState state)
        {
            switch (state)
            {
                case GTM.Module.NetworkModule.NetworkState.Down:
                    Debug.Print("Network Down Event - Down");
                    break;
                case GTM.Module.NetworkModule.NetworkState.Up:
                    Debug.Print("Network Down Event - Up");
                    break;
            }
        }


        private void _timer_Tick(GT.Timer timer)
        {
            Mainboard.SetDebugLED(true);

            var measurement = tempHumidSI70.TakeMeasurement();
            var volts = lightSense.ReadVoltage();

            // Display data and time in debug console
            Debug.Print(DateTime.Now.ToString());
            Debug.Print("Temperature = " + measurement.TemperatureFahrenheit);
            Debug.Print("Humidity = " + measurement.RelativeHumidity);
            Debug.Print("Light = " + volts);

            // send to Event Hub
            SendAMQPMessage(FormatMessage("Temperature", "F", measurement.TemperatureFahrenheit));
            SendAMQPMessage(FormatMessage("Humidity", "%", measurement.RelativeHumidity));
            SendAMQPMessage(FormatMessage("Light", "v", volts));

            Mainboard.SetDebugLED(false);
        }

        // Initialization of AMQP connection to Azure Event Hubs
        private void InitAMQPconnection()
        {
            // Get the Event Hub URI
            _address = new Address(AMQPAddress);

            // create connection
            _connection = new Connection(_address);

            // create session
            _session = new Session(_connection);

            // create sender
            _sender = new SenderLink(_session, "send-link", EventHub);
        }

        private string FormatMessage(string measureName, string unitOfMeasure, double value)
        {
            // Create hashtable for data
            var hashtable = new Hashtable();
            hashtable.Add("organization", Organization);
            hashtable.Add("location", Location);
            hashtable.Add("guid", SensorGUID);
            hashtable.Add("displayname", SensorName);
            hashtable.Add("unitofmeasure", unitOfMeasure);
            hashtable.Add("measurename", measureName);
            hashtable.Add("value", value);
            hashtable.Add("timecreated", DateTime.UtcNow);

            // Serialize hashtable into JSON
            var serializer = new JsonSerializer(DateTimeFormat.Default);
            var payload = serializer.Serialize(hashtable);

            return payload;
        }

        // Send a message to Azure Event Hubs using AMQP protocol
        // we are using the sender resource created in the initialization
        private void SendAMQPMessage(string payload)
        {
            try
            {
                // 0 indicates the method is not in use
                if (0 == Interlocked.Exchange(ref _isSendingMessage, 1))
                {
                    // Create the AMQP message
                    var encodedTextData = Encoding.UTF8.GetBytes(payload);
                    var message = new Message // Message
                    {
                        BodySection = new Data
                        {
                            Binary = encodedTextData
                        },
                        Properties = new Properties
                        {
                            CreationTime = DateTime.UtcNow,
                            ContentType = "text/json"
                        }
                    };

                    message.MessageAnnotations = new MessageAnnotations();
                    message.MessageAnnotations[new Symbol("x-opt-partition-key")] = SensorGUID;
                    message.ApplicationProperties = new ApplicationProperties();

                    _sender.Send(message);
                }
            }
            catch (Exception e)
            {
                Debug.Print("Exception caught:" + e.Message);
            }
            finally
            {
                // release lock
                Interlocked.Exchange(ref _isSendingMessage, 0);
            }
        }

        public static bool NTPTime(string TimeServer, int GmtOffset = 0)
        {
            Socket s = null;
            try
            {
                EndPoint rep = new IPEndPoint(Dns.GetHostEntry(TimeServer).AddressList[0], 123);
                s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                var ntpData = new byte[48];
                Array.Clear(ntpData, 0, 48);
                ntpData[0] = 0x1B; // Set protocol version
                s.SendTo(ntpData, rep); // Send Request   
                if (s.Poll(30 * 1000 * 1000, SelectMode.SelectRead)) // Waiting an answer for 30s, if nothing: timeout
                {
                    s.ReceiveFrom(ntpData, ref rep); // Receive Time
                    byte offsetTransmitTime = 40;
                    ulong intpart = 0;
                    ulong fractpart = 0;
                    for (var i = 0; i <= 3; i++) intpart = (intpart << 8) | ntpData[offsetTransmitTime + i];
                    for (var i = 4; i <= 7; i++) fractpart = (fractpart << 8) | ntpData[offsetTransmitTime + i];
                    var milliseconds = (intpart * 1000 + (fractpart * 1000) / 0x100000000L);
                    s.Close();
                    var dateTime = new DateTime(1900, 1, 1) +
                                   TimeSpan.FromTicks((long)milliseconds * TimeSpan.TicksPerMillisecond);

                    Utility.SetLocalTime(dateTime); //sets the time as UTC

                    return true;
                }
                s.Close();
            }
            catch
            {
                try
                {
                    s.Close();
                }
                catch
                {
                }
            }
            return false;
        }
    }
}