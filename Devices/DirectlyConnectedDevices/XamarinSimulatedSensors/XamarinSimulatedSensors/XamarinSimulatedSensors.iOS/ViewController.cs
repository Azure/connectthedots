using System;

using UIKit;

namespace XamarinSimulatedSensors.iOS
{
	public partial class ViewController : UIViewController
	{
        MyClass Device;

		public ViewController (IntPtr handle) : base (handle)
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
            // Perform any additional setup after loading the view, typically from a nib.
            Device = new MyClass();

            // If you are developing and want to avoid having to enter the full connection string on the device,
            // you can temporarily hard code it here. Comment this when done!
            //Device.DisplayName = "[DisplayName]";
            //Device.ConnectionString = "[ConnectionString]";

            buttonConnect.Enabled = false;
            buttonConnect.TouchUpInside += ButtonConnect_TouchUpInside;

            buttonSend.Enabled = false;
            buttonSend.TouchUpInside += ButtonSend_TouchUpInside;

            textDisplayName.EditingDidEnd += TextDisplayName_EditingDidEnd;
            textDisplayName.Text = Device.DisplayName;

            textConnectionString.EditingDidEnd += TextConnectionString_EditingDidEnd;
            textConnectionString.Text = Device.ConnectionString;

            buttonConnect.Enabled = Device.checkConfig();

            sliderTemperature.MinValue = 0;
            sliderTemperature.MaxValue = 100;
            sliderTemperature.ValueChanged += SliderTemperature_ValueChanged;
            sliderTemperature.Value = 50;

            sliderHumidity.MinValue = 0;
            sliderHumidity.MaxValue = 100;
            sliderHumidity.ValueChanged += SliderHumidity_ValueChanged;
            sliderHumidity.Value = 50;         

		}


        private void SliderHumidity_ValueChanged(object sender, EventArgs e)
        {
            lblHumidity.Text = "Humidity: " + sliderHumidity.Value.ToString();
            Device.UpdateSensorData("Humidity", sliderHumidity.Value);
        }

        private void SliderTemperature_ValueChanged(object sender, EventArgs e)
        {
            lblTemperature.Text = "Temperature: " + sliderTemperature.Value.ToString();
            Device.UpdateSensorData("Temperature", sliderTemperature.Value);
        }

        private void TextConnectionString_EditingDidEnd(object sender, EventArgs e)
        {
            Device.ConnectionString = textConnectionString.Text;
            buttonConnect.Enabled = Device.checkConfig();
        }

        private void TextDisplayName_EditingDidEnd(object sender, EventArgs e)
        {
            Device.DisplayName = textDisplayName.Text;
            buttonConnect.Enabled = Device.checkConfig();
        }

        private void ButtonSend_TouchUpInside(object sender, EventArgs e)
        {
            if (Device.SendTelemetryData)
            {
                Device.SendTelemetryData = false;
                buttonSend.SetTitle("Press to send telemetry data", UIControlState.Normal);
            }
            else
            {
                Device.SendTelemetryData = true;
                buttonSend.SetTitle("Sending telemetry data", UIControlState.Normal);
            }
        }

        private void ButtonConnect_TouchUpInside(object sender, EventArgs e)
        {
            if (Device.IsConnected)
            {
                Device.SendTelemetryData = false;
                if (Device.Disconnect())
                {
                    buttonSend.Enabled = false;
                    textDisplayName.Enabled = true;
                    textConnectionString.Enabled = true;
                    buttonConnect.SetTitle("Press to connect the dots", UIControlState.Normal);
                }
            }
            else
            {
                if (Device.Connect())
                {
                    buttonSend.Enabled = true;
                    textDisplayName.Enabled = false;
                    textConnectionString.Enabled = false;
                    buttonConnect.SetTitle("Dots connected", UIControlState.Normal);

                }
            }

        }

        public override void DidReceiveMemoryWarning ()
		{
			base.DidReceiveMemoryWarning ();
			// Release any cached data, images, etc that aren't in use.
		}
	}
}

