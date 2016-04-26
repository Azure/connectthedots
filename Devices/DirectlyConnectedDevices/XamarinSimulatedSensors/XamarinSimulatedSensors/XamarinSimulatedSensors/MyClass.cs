using System;
using System.Text;
using System.Diagnostics;
using ConnectTheDotsHelper;
using XamarinSimulatedSensors.Helpers;

namespace XamarinSimulatedSensors
{
    public class MyClass:ConnectTheDots
    {
        public bool Connected { get; set; } = false;
        public bool Sending { get; set; } = false;

        public MyClass()
        {
            this.DisplayName = Settings.DeviceName;
            this.ConnectionString = Settings.DeviceConnectionString;
            this.Organization = "Microsoft";
            this.Location = "My Location";

            this.AddSensor("Temperature", "C");
            this.AddSensor("Humidity", "%");
        }

        public bool checkConfig()
        {
            return ((this.DisplayName != null) && (this.ConnectionString != null) &&
                    (this.DisplayName != "") && (this.ConnectionString != ""));
        }

        public void UpdateSensorData(string SensorName, double value)
        {
            if (this.Sensors.ContainsKey(SensorName))
                this.Sensors[SensorName].value = value;
        }

        public bool CTDConnect()
        {
            Connected = this.Connect();
            return Connected;
        }
        public bool CTDDisconnect()
        {
            Connected = !this.Disconnect();
            return !Connected;
        }

    }
}

