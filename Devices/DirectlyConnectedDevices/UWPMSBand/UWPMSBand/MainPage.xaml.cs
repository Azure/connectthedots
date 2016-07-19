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
using System.Text.RegularExpressions;
using Microsoft.Band;
using Microsoft.Band.Sensors;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using ConnectTheDotsHelper;

namespace UWPMSBand
{
    public sealed partial class MainPage : Page
    {
        private GeolocationAccessStatus LocationAccess = GeolocationAccessStatus.Unspecified;
        private Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
        private ConnectTheDots CTD;

        //private DeviceClient deviceClient;
        private IBandClient bandClient;


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

            // Connect to MS Band
            connectToBand();

            // Start task that will send telemetry data to Azure
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
        /// Check the validity of the connection string and display name.
        /// </summary>
        private bool checkConfig()
        {
            return ((this.TBDeviceName.Text != null) && (this.TBConnectionString.Text != null) &&
                    (this.TBDeviceName.Text != "") && (this.TBConnectionString.Text != ""));
        }

        /// <summary>
        /// Update device's current location
        /// </summary>
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

        private void toggleButton_Checked(object sender, RoutedEventArgs e)
        {
            SendDataToggle.Content = "Sending telemetry data";
            CTD.SendTelemetryData = true;
        }

        private void toggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            SendDataToggle.Content = "Press to send telemetry data";
            CTD.SendTelemetryData = false;
        }

        private void TBDeviceName_TextChanged(object sender, TextChangedEventArgs e)
        {
            CTD.DisplayName = TBDeviceName.Text;
            localSettings.Values["DisplayName"] = CTD.DisplayName;
            ConnectToggle.IsEnabled = checkConfig();

        }

        private void TBConnectionString_TextChanged(object sender, TextChangedEventArgs e)
        {
            CTD.ConnectionString = TBConnectionString.Text;
            localSettings.Values["ConnectionString"] = CTD.ConnectionString;
            ConnectToggle.IsEnabled = checkConfig();
        }

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


        private async void SkinTemperature_ReadingChanged(object sender, BandSensorReadingEventArgs<IBandSkinTemperatureReading> args)
        {
            // do work when the reading changes (i.e., update a UI element)
            CTD.Sensors["SkinTemperature"].value = args.SensorReading.Temperature;

            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                async () =>
                {
                    tbSkinTemperature.Text = string.Format("{0:0}", CTD.Sensors["SkinTemperature"].value);
                });

            Debug.WriteLine("SkinTemperature Changed: " + string.Format("{0:0}", CTD.Sensors["SkinTemperature"].value));
        }

        private async void HeartRate_ReadingChanged(object sender, BandSensorReadingEventArgs<IBandHeartRateReading> args)
        {
            // do work when the reading changes (i.e., update a UI element)
            CTD.Sensors["HeartRate"].value = args.SensorReading.HeartRate;
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                async () =>
                {
                    tbHeartRate.Text = string.Format("{0:0}", CTD.Sensors["HeartRate"].value);
                });

            Debug.WriteLine("HeartRate Changed: " + string.Format("{0:0}", CTD.Sensors["HeartRate"].value));
        }

        private async void AddBandSensor<T>(IBandSensor<T> sensor,
                                         string measurename,
                                         string unitofmeasure,
                                         EventHandler<BandSensorReadingEventArgs<T>> ValueChangedEventHandler) where T : IBandSensorReading
        {

            // check current user consent for accessing Band sensor
            if (sensor.GetCurrentUserConsent() != UserConsent.Granted)
            {
                // user hasn’t consented, request consent  
                await sensor.RequestUserConsentAsync();
                if (sensor.GetCurrentUserConsent() != UserConsent.Granted) return;
            }
            // User granted consent
            // Add Sensor to ConnectTheDots Helper
            CTD.AddSensor(measurename, unitofmeasure);

            // hook up to the Sensor ReadingChanged event 
            IEnumerable<TimeSpan> supportedHeartRateReportingIntervals = sensor.SupportedReportingIntervals;
            sensor.ReportingInterval = supportedHeartRateReportingIntervals.First<TimeSpan>();
            sensor.ReadingChanged += ValueChangedEventHandler;

            // start reading from the sensor
            await sensor.StartReadingsAsync();
        }

        private async void connectToBand()
        {
            try
            {
                IBandInfo[] pairedBands = await BandClientManager.Instance.GetBandsAsync();

                bandClient = await BandClientManager.Instance.ConnectAsync(pairedBands[0]);
                AddBandSensor<IBandHeartRateReading>(bandClient.SensorManager.HeartRate,
                                    "HeartRate",
                                    "bpm",
                                    HeartRate_ReadingChanged);

                AddBandSensor<IBandSkinTemperatureReading>(bandClient.SensorManager.SkinTemperature,
                                    "SkinTemperature",
                                    "C",
                                    SkinTemperature_ReadingChanged);
            }
            catch (BandException ex)
            {
                // handle a Band connection exception 
                var dialogbox = new MessageDialog("Error while connecting Band Sensors: " + ex.Message.ToString() + "\nRetrying...");
                await dialogbox.ShowAsync();
                connectToBand();
            }
        }

    }
}
