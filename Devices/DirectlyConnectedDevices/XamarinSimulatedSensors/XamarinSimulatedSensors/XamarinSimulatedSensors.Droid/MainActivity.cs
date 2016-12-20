using System;
using System.Threading.Tasks;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

using ZXing.Mobile;

namespace XamarinSimulatedSensors.Droid
{
	[Activity (Label = "XamarinSimulatedSensors.Droid", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : Activity
	{
        MyClass Device;

        Button buttonConnect;
        Button buttonSend;
        Button buttonScan;
        TextView textDeviceName;
        TextView textConnectionString;

        TextView lblTemperature;
        TextView lblHumidity;

        TextView textAlerts;

        protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

            // Initialize IoT Hub client
            Device = new MyClass();

            // If you are developing and want to avoid having to enter the full connection string on the device,
            // you can temporarily hard code it here. Comment this when done!
            //Device.DisplayName = "[DisplayName]";
            //Device.ConnectionString = "[ConnectionString]";

            // Prepare UI elements
            buttonConnect = FindViewById<Button>(Resource.Id.buttonConnect);
            buttonConnect.Enabled = false;
            buttonConnect.Click += ButtonConnect_Click;

            buttonSend = FindViewById<Button>(Resource.Id.buttonSend);
            buttonSend.Enabled = false;
            buttonSend.Click += ButtonSend_Click;

            buttonScan = FindViewById<Button>(Resource.Id.buttonScan);
            buttonScan.Enabled = true;
            buttonScan.Click += ButtonScan_Click;

            textDeviceName = FindViewById<TextView>(Resource.Id.textDeviceName);
            textDeviceName.TextChanged += TextDeviceName_TextChanged;
            textDeviceName.Text = Device.DisplayName;

            textConnectionString = FindViewById<TextView>(Resource.Id.textConnectionString);
            textConnectionString.TextChanged += TextConnectionString_TextChanged;
            textConnectionString.SetSingleLine(true);
            textConnectionString.Text = Device.ConnectionString;

            lblTemperature = FindViewById<TextView>(Resource.Id.lblTemperature);
            SeekBar seekTemperature = FindViewById<SeekBar>(Resource.Id.seekBarTemperature);
            seekTemperature.ProgressChanged += SeekTemperature_ProgressChanged;
            seekTemperature.Progress = 50;

            lblHumidity = FindViewById<TextView>(Resource.Id.lblHumidity);
            SeekBar seekHumidity = FindViewById<SeekBar>(Resource.Id.seekBarHumidity);
            seekHumidity.ProgressChanged += SeekHumidity_ProgressChanged;
            seekHumidity.Progress = 50;

            textAlerts = FindViewById<TextView>(Resource.Id.textAlerts);

            Device.ReceivedMessage += Device_ReceivedMessage;
            // Set focus to the connect button
            buttonConnect.RequestFocus();
        }

        /// <summary>
        /// ScanCode
        /// Scan a QR Code using the ZXing library
        /// </summary>
        /// <returns></returns>
        private async Task ScanCode()
        {
            MobileBarcodeScanner.Initialize(Application);

            MobileBarcodeScanner Scanner = new MobileBarcodeScanner();
            Scanner.UseCustomOverlay = false;
            Scanner.TopText = "Hold camera up to QR code";
            Scanner.BottomText = "Camera will automatically scan QR code\r\n\rPress the 'Back' button to cancel";

            ZXing.Result result = await Scanner.Scan().ConfigureAwait(true);

            if (result == null || (string.IsNullOrEmpty(result.Text)))
            {
                //await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                //{
                    //MessageDialog dialog = new MessageDialog("An error occured while scanning the QRCode. Try again");
                    //await dialog.ShowAsync();
                //});
            }
            else
            {
                //await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                //{
                    textConnectionString.Text = result.Text;
                    textDeviceName.Text = Device.ExtractDeviceIdFromConnectionString(result.Text);
                //});
            }
        }

        private async void ButtonScan_Click(object sender, EventArgs e)
        {
            if (buttonSend.Enabled == false)
            {
                await ScanCode();
            }
        }

        private void Device_ReceivedMessage(object sender, EventArgs e)
        {
            ConnectTheDotsHelper.C2DMessage message = ((ConnectTheDotsHelper.ConnectTheDots.ReceivedMessageEventArgs)e).Message;
            var textToDisplay = message.timecreated + " - Alert received:" + message.message + ": " + message.value + " " + message.unitofmeasure + "\r\n";

            textAlerts.Append(textToDisplay);
        }

        private void SeekHumidity_ProgressChanged(object sender, SeekBar.ProgressChangedEventArgs e)
        {
            lblHumidity.Text = "Humidity: " + e.Progress;
            Device.UpdateSensorData("Humidity", e.Progress);
        }

        private void SeekTemperature_ProgressChanged(object sender, SeekBar.ProgressChangedEventArgs e)
        {
            lblTemperature.Text = "Temperature: " + e.Progress;
            Device.UpdateSensorData("Temperature", e.Progress);
        }

        private void ButtonSend_Click(object sender, EventArgs e)
        {
            if (Device.SendTelemetryData)
            {
                
                Device.SendTelemetryData = false;
                buttonSend.Text = "Press to send telemetry data";
            }
            else
            {
                Device.SendTelemetryData = true;
                buttonSend.Text = "Sending telemetry data";
            }
        }

        private void ButtonConnect_Click(object sender, EventArgs e)
        {
            if (Device.IsConnected)
            {
                Device.SendTelemetryData = false;
                if (Device.Disconnect())
                {
                    buttonSend.Enabled = false;
                    textDeviceName.Enabled = true;
                    textConnectionString.Enabled = true
                    buttonScan.Enabled = true;
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
                    buttonScan.Enabled = false;
                    buttonConnect.Text = "Dots connected";

                }
            }

        }

        private void TextConnectionString_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            Device.ConnectionString = e.Text.ToString();
            buttonConnect.Enabled = Device.checkConfig();
        }

        private void TextDeviceName_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            Device.DisplayName = e.Text.ToString();
            buttonConnect.Enabled = Device.checkConfig();
        }
    }
}


