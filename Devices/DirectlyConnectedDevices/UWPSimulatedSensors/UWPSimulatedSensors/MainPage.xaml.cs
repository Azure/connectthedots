using System;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.Devices.Geolocation;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
using System.Text.RegularExpressions;
using ConnectTheDotsHelper;


namespace UWPSimulatedSensors
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private GeolocationAccessStatus LocationAccess = GeolocationAccessStatus.Unspecified;
        private Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
        private ConnectTheDots CTD;

        public MainPage()
        {
            this.InitializeComponent();

            // Initialize ConnectTheDots Helper
            CTD = new ConnectTheDots();

            // Restore local settings
            if (localSettings.Values.ContainsKey("DisplayName"))
            {
                CTD.DisplayName = (string)localSettings.Values["DisplayName"];
                this.TBDeviceName.Text = CTD.DisplayName;
            }
            if (localSettings.Values.ContainsKey("ConnectionString"))
            {
                CTD.ConnectionString = (string)localSettings.Values["ConnectionString"];
                this.TBConnectionString.Text = CTD.ConnectionString;
            }

            // Check configuration settings
            ConnectToggle.IsEnabled = checkConfig();
            CTD.DisplayName = this.TBDeviceName.Text;
            CTD.ConnectionString = this.TBConnectionString.Text;
            CTD.Organization = "My Company";
            CTD.Location = "Unknown";

            // Get user consent for accessing location
            Task.Run(async () =>
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                async () =>
                {
                    this.LocationAccess = await Geolocator.RequestAccessAsync();
                });
            });

            // Add sensors to the ConnectTheDots object
            CTD.AddSensor("Temperature", "C");
            CTD.AddSensor("Humidity", "%");

            // Hook up a callback to display message received from Azure
            CTD.ReceivedMessage += (object sender, EventArgs e) => {
                // Received a new message, display it
                CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                async () =>
                {
                    var dialogbox = new MessageDialog("Received message from Azure IoT Hub: \nName: " +
                                                      ((ConnectTheDots.ReceivedMessageEventArgs)e).Message.name +
                                                      "\nMessage: " +
                                                      ((ConnectTheDots.ReceivedMessageEventArgs)e).Message.message);
                    await dialogbox.ShowAsync();
                });
            };
        }

        /// <summary>
        /// checkConfig
        /// Check stored configuration
        /// </summary>
        /// <returns></returns>
        private bool checkConfig()
        {
            return ((this.TBDeviceName.Text != null) && (this.TBConnectionString.Text != null) &&
                    (this.TBDeviceName.Text != "") && (this.TBConnectionString.Text != ""));
        }

        /// <summary>
        /// updateLocation
        /// Updates current location of the device
        /// </summary>
        /// <returns></returns>
        private async Task updateLocation()
        {
            // Update current device location
            try
            {
                if (LocationAccess == GeolocationAccessStatus.Allowed)
                {

                    Geolocator geolocator = new Geolocator();
                    Geoposition pos = await geolocator.GetGeopositionAsync();
                    CTD.Location = pos.Coordinate.Point.Position.Longitude.ToString() + "," + pos.Coordinate.Point.Position.Latitude.ToString();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error while trying to retreive device's location: " + ex.Message);
                CTD.Location = "unknown";
            }
        }

        /// <summary>
        /// toggleButton_Checked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toggleButton_Checked(object sender, RoutedEventArgs e)
        {
            SendDataToggle.Content = "Sending telemetry data";
            CTD.SendTelemetryData = true;
        }
        /// <summary>
        /// toggleButton_Unchecked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            SendDataToggle.Content = "Press to send telemetry data";
            CTD.SendTelemetryData = false;
        }
        /// <summary>
        /// TempSlider_ValueChanged
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TempSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if ((CTD !=null) && (CTD.Sensors["Temperature"] != null))
                CTD.Sensors["Temperature"].value = TempSlider.Value;
        }
        /// <summary>
        /// HmdtSlider_ValueChanged
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HmdtSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if ((CTD != null) && (CTD.Sensors["Humidity"] != null))
                CTD.Sensors["Humidity"].value = HmdtSlider.Value;
        }
        /// <summary>
        /// TBDeviceName_TextChanged
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TBDeviceName_TextChanged(object sender, TextChangedEventArgs e)
        {
            CTD.DisplayName = TBDeviceName.Text;
            localSettings.Values["DisplayName"] = CTD.DisplayName;
            ConnectToggle.IsEnabled = checkConfig();

        }
        /// <summary>
        /// TBConnectionString_TextChanged
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TBConnectionString_TextChanged(object sender, TextChangedEventArgs e)
        {
            CTD.ConnectionString = TBConnectionString.Text;
            localSettings.Values["ConnectionString"] = CTD.ConnectionString;
            ConnectToggle.IsEnabled = checkConfig();
        }
        /// <summary>
        /// ConnectToggle_Checked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConnectToggle_Checked(object sender, RoutedEventArgs e)
        {
            if (CTD.Connect())
            {
                SendDataToggle.IsEnabled = true;
                TBDeviceName.IsEnabled = false;
                TBConnectionString.IsEnabled = false;
                ConnectToggle.Content = "Dots connected";
            }
        }
        /// <summary>
        /// ConnectToggle_Unchecked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConnectToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            if (CTD.Disconnect())
            {
                SendDataToggle.IsChecked = false;
                SendDataToggle.IsEnabled = false;
                TBDeviceName.IsEnabled = true;
                TBConnectionString.IsEnabled = true;
                ConnectToggle.Content = "Press to connect the dots";
            }
        }
    }
}
