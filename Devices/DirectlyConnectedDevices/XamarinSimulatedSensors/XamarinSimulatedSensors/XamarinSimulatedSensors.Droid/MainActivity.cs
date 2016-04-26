using System;
using System.Threading.Tasks;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace XamarinSimulatedSensors.Droid
{
	[Activity (Label = "XamarinSimulatedSensors.Droid", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : Activity
	{
		int count = 1;

        MyClass Device;

        Button buttonConnect;
        Button buttonSend;
        TextView textDeviceName;
        TextView textConnectionString;

        protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

            // Initialize IoT Hub client
            Device = new MyClass();

            Device.DisplayName = "Droid";
            Device.ConnectionString = "HostName=ctdoli1010hub.azure-devices.net;DeviceId=Droid;SharedAccessKey=mBg1YJbnl2SrvDuE+ix7ZIwimsfaJ9aDidiuCpCe3l0=";


            // Set the callbacks for the text fields changes
            textDeviceName = FindViewById<TextView>(Resource.Id.textDeviceName);
            textDeviceName.Text = Device.DisplayName;
            textDeviceName.TextChanged += (object sender, Android.Text.TextChangedEventArgs e) =>
            {
                Device.DisplayName = e.Text.ToString();
            };

            textConnectionString = FindViewById<TextView>(Resource.Id.textConnectionString);
            textConnectionString.Text = Device.ConnectionString;
            textConnectionString.TextChanged += (object sender, Android.Text.TextChangedEventArgs e) =>
            {
                Device.ConnectionString = e.Text.ToString();
            };

            buttonConnect = FindViewById<Button>(Resource.Id.buttonConnect);
            buttonConnect.Enabled = Device.checkConfig();
            buttonConnect.Click += (object sender, EventArgs e)=>
            {
                if (Device.Connected)
                {
                    if (Device.CTDDisconnect())
                    {
                        buttonSend.Enabled = false;
                        textDeviceName.Enabled = true;
                        textConnectionString.Enabled = true;
                        buttonConnect.Text = "Press to connect the dots";
                    }
                }
                else
                {
                    if (Device.Connect())
                    {
                        buttonSend.Enabled = true;
                        textDeviceName.Enabled = false;
                        textConnectionString.Enabled = false;
                        buttonConnect.Text = "Dots connected";

                    }
                }
            };

            buttonSend = FindViewById<Button>(Resource.Id.buttonSend);
            buttonSend.Enabled = false;
            buttonSend.Click += (object sender, EventArgs e) =>
            {
                if (Device.Sending)
                {
                    Device.SendTelemetryData = false;
                    Device.Sending = false;
                    buttonSend.Text = "Press to send telemetry data";
                }
                else
                {
                    Device.SendTelemetryData = true;
                    Device.Sending = true;
                    buttonSend.Text = "Sending telemetry data";
                }
            };

            // Set the Temperature UI
            TextView lblTemperature = FindViewById<TextView>(Resource.Id.lblTemperature);
            SeekBar seekTemperature = FindViewById<SeekBar>(Resource.Id.seekBarTemperature);
            seekTemperature.Progress = 50;

            Device.UpdateSensorData("Temperature", seekTemperature.Progress);

            seekTemperature.ProgressChanged += (object sender, SeekBar.ProgressChangedEventArgs e) =>
            {
                lblTemperature.Text = "Temperature: " + e.Progress;
                Device.UpdateSensorData("Temperature", e.Progress);
            };

            // Set the Humidity UI
            TextView lblHumidity = FindViewById<TextView>(Resource.Id.lblHumidity);
            SeekBar seekHumidity = FindViewById<SeekBar>(Resource.Id.seekBarHumidity);
            seekHumidity.Progress = 50;

            Device.UpdateSensorData("Humidity", seekHumidity.Progress);

            seekHumidity.ProgressChanged += (object sender, SeekBar.ProgressChangedEventArgs e) =>
            {
                lblHumidity.Text = "Humidity: " + e.Progress;
                Device.UpdateSensorData("Humidity", e.Progress);
            };
		}

    }
}


