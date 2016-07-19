using System;
using System.Text;
using System.Diagnostics;
using ConnectTheDotsHelper;
using XamarinSimulatedSensors.Helpers;

namespace XamarinSimulatedSensors
{
    public class MyClass:ConnectTheDots
    {
        public MyClass()
        {
            this.DisplayName = Settings.DisplayName;
            this.ConnectionString = Settings.ConnectionString;
            this.Organization = "Microsoft";
            this.Location = "My Location";

            this.AddSensor("Temperature", "C");
            this.AddSensor("Humidity", "%");
        }

        public bool checkConfig()
        {
            if (((this.DisplayName != null) && (this.ConnectionString != null) &&
                        (this.DisplayName != "") && (this.ConnectionString != "")))
            {
                Settings.DisplayName = this.DisplayName;
                Settings.ConnectionString = this.ConnectionString;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void UpdateSensorData(string SensorName, double value)
        {
            if (this.Sensors.ContainsKey(SensorName))
                this.Sensors[SensorName].value = value;
        }
    }
}

