using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ConnectTheDotsHelper
{
    // Data contract defining Connect The Dots data telemetry format
    [DataContract]
    public class D2CMessage
    {
        [DataMember]
        internal string guid;

        [DataMember]
        internal string displayname;

        [DataMember]
        internal string organization;

        [DataMember]
        internal string location;

        [DataMember]
        internal string measurename;

        [DataMember]
        internal string unitofmeasure;

        [DataMember]
        internal string timecreated;

        [DataMember]
        internal double value;
    }

    // Data contract defining Connect The Dots Cloud to Device message format
    [DataContract]
    public class C2DMessage
    {
        [DataMember]
        internal string name;

        [DataMember]
        internal string message;
    }

    /// <summary>
    /// ConnectTheDots class
    /// Provides helper functions for easily connect a device to Azure IoT Hub and send and receive messages to and from a ConnectTheDots website
    /// </summary>
    public class ConnectTheDots
    {
        // Azure IoT Hub client
        private DeviceClient deviceClient;

        // Collection of sensors
        public Dictionary<string, D2CMessage> Sensors { get; set; } = new Dictionary<string, D2CMessage>();
        public void AddSensor(string MeasureName, string UnitOfMeasure)
        {
            Sensors.Add(MeasureName, new D2CMessage
            {
                guid = Guid,
                displayname = DisplayName,
                location = Location,
                organization = Organization,
                measurename = MeasureName,
                unitofmeasure = UnitOfMeasure,
                timecreated = DateTime.UtcNow.ToString("o"),
                value = 0
            });
        }

        // ConnectTheDots properties
        private string _ConnectionString;
        public string ConnectionString {
            get
            {
                return _ConnectionString;
            }
            set
            {
                this._ConnectionString = value;
                this.Guid = ExtractDeviceIdFromConnectionString(value);
            }
        }
        public string Guid { get; set; }
        public string DisplayName { get; set; }
        public string Organization { get; set; }
        public string Location { get; set; }
        public bool SendTelemetryData { get; set; }
        public int SendTelemetryFreq { get; set; } = 1000;
        public bool IsConnected { get; set; } = false;

        // Sending and receiving tasks
        CancellationTokenSource TokenSource = new CancellationTokenSource();

        // Event Handler for notifying the reception of a new message from IoT Hub
        public event EventHandler ReceivedMessage;

        /// <summary>
        /// ReceivedMessageEventArgs class
        /// Class to pass event arguments for new message received from ConnectTheDots dashboard
        /// </summary>
        public class ReceivedMessageEventArgs : System.EventArgs
        {
            // Provide one or more constructors, as well as fields and
            // accessors for the arguments.
            public C2DMessage Message { get; set; }

            public ReceivedMessageEventArgs(C2DMessage message)
            {
                Message = message;
            }
        }

        // Trigger for notifying reception of new message from Connect The Dots dashboard
        protected virtual void OnReceivedMessage(ReceivedMessageEventArgs e)
        {
            if (ReceivedMessage != null)
                ReceivedMessage(this, e);
        }

        /// <summary>
        /// Serialize message
        /// </summary>
        private byte[] Serialize(object obj)
        {
            string json = JsonConvert.SerializeObject(obj);
            return Encoding.UTF8.GetBytes(json);

        }

        /// <summary>
        /// DeSerialize message
        /// </summary>
        private dynamic DeSerialize(byte[] data)
        {
            string text = Encoding.UTF8.GetString(data, 0, data.Length);
            return JsonConvert.DeserializeObject(text);
        }

        /// <summary>
        /// Send device's telemetry data to Azure IoT Hub
        /// </summary>
        public async void sendDeviceTelemetryData(D2CMessage[] data)
        {
            try
            {
                var msg = new Message(Serialize(data));
                if (deviceClient != null)
                {
                    await deviceClient.SendEventAsync(msg);
                    Debug.WriteLine("Sent telemetry data to IoT Hub\n" + data.ToString());
                }
                else Debug.WriteLine("Connection To IoT Hub is not established. Cannot send message now");
                
            }
            catch (System.Exception e)
            {
                Debug.WriteLine("Exception while sending device telemetry data :\n" + e.Message.ToString());
            }
        }

        /// <summary>
        /// ExtractDeviceIdFromConnectionString
        /// Extract DeviceId from connectionstring to use as guid in ConnectTheDots dashboard
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        private string ExtractDeviceIdFromConnectionString(string connectionString)
        {
            Regex pattern = new Regex(@"HostName=(?<hostName>[^\s/]*);DeviceId=(?<deviceId>[^\s/]*);SharedAccessKey=(?<shareAccessKey>[^\s/]*)");
            Match match = pattern.Match(connectionString);
            return match.Groups["deviceId"].Value;
        }

        /// <summary>
        /// Connect
        /// Connect to Azure IoT Hub ans start the send and receive loops
        /// </summary>
        /// <returns></returns>
        public bool Connect()
        {
            try
            {
                // Create Azure IoT Hub Client and open messaging channel
                deviceClient = DeviceClient.CreateFromConnectionString(this.ConnectionString);
                deviceClient.OpenAsync();
                IsConnected = true;

                // Create send and receive tasks
                CancellationToken ct = TokenSource.Token;
                Task.Factory.StartNew(async()=> {
                    while (true)
                    {
                        if (SendTelemetryData)
                        {
                            // Create message to be sent
                            D2CMessage[] dataToSend = new D2CMessage[Sensors.Count];
                            int index = 0;

                            foreach (KeyValuePair<string, D2CMessage> sensor in Sensors)
                            {
                                // Update the values that 
                                sensor.Value.displayname = DisplayName;
                                sensor.Value.location = Location;
                                sensor.Value.timecreated = DateTime.UtcNow.ToString("o");
                                dataToSend[index++] = sensor.Value;
                            }
                            // Send message
                            sendDeviceTelemetryData(dataToSend);
                        }
                        await Task.Delay(SendTelemetryFreq);

                        if (ct.IsCancellationRequested)
                        {
                            // Cancel was called
                            Debug.WriteLine("Sending task canceled");
                            break;
                        }

                    }
                }, ct);

                Task.Factory.StartNew(async() =>
                {
                    while (true)
                    {
                        // Receive message from Cloud (for now this is a pull because only HTTP is available for UWP applications)
                        Message message = await deviceClient.ReceiveAsync();
                        if (message != null)
                        {
                            try
                            {
                                // Read message and deserialize
                                C2DMessage command = DeSerialize(message.GetBytes());

                                // Invoke message received callback
                                OnReceivedMessage(new ReceivedMessageEventArgs(command));

                                // We received the message, indicate IoTHub we treated it
                                await deviceClient.CompleteAsync(message);
                            }
                            catch
                            {
                                // Something went wrong. Indicate the backend that we coudn't accept the message
                                await deviceClient.RejectAsync(message);
                            }
                        }
                        if (ct.IsCancellationRequested)
                        {
                            // Cancel was called
                            Debug.WriteLine("Receiving task canceled");
                            break;
                        }
                    }
                }, ct);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error while trying to connect to IoT Hub: " + e.Message);
                deviceClient = null;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Disconnect
        /// Disconnect from IoT Hub
        /// </summary>
        /// <returns></returns>
        public bool Disconnect()
        {
            if (deviceClient != null)
            {
                try
                {
                    deviceClient.CloseAsync();
                    deviceClient = null;
                    IsConnected = false;
                }
                catch
                {
                    Debug.WriteLine("Error while trying close the IoT Hub connection");
                    return false;
                }
            }
            return true;
        }
    }
}
