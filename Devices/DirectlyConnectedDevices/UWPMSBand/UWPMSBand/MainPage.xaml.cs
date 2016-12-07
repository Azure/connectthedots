using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.Devices.Geolocation;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Microsoft.Band;
using Microsoft.Band.Sensors;
using System.Collections.Generic;
using System.Linq;
using ConnectTheDotsHelper;

namespace UWPMSBand
{
    public sealed partial class MainPage : Page
    {
        private GeolocationAccessStatus LocationAccess = GeolocationAccessStatus.Unspecified;
        private Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
        private ConnectTheDots CTD;
        private delegate void AppendAlert(string AlertText);

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

            // Hook up a callback to display message received from Azure
            CTD.ReceivedMessage += CTD_ReceivedMessage;

            // Get user consent for accessing location
            Task.Run(async () =>
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                async () =>
                {
                    this.LocationAccess = await Geolocator.RequestAccessAsync();
                    // Get device location
                    await updateLocation();
                });
            });

            // Connect to MS Band
            Task.Run(async () =>
            {
                await connectToBand();
            });
        }

        private void CTD_ReceivedMessage(object sender, EventArgs e)
        {
            CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
            async () =>
            {
                ConnectTheDotsHelper.C2DMessage message = ((ConnectTheDotsHelper.ConnectTheDots.ReceivedMessageEventArgs)e).Message;
                var textToDisplay = message.timecreated + " - Alert received:" + message.message + ": " + message.value + " " + message.unitofmeasure + "\r\n";
                TBAlerts.Text += textToDisplay;
            });

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


        private async void SkinTemperature_ReadingChanged(object sender, BandSensorReadingEventArgs<IBandSkinTemperatureReading> args)
        {
            // do work when the reading changes (i.e., update a UI element)
            CTD.Sensors["SkinTemperature"].message.value = args.SensorReading.Temperature;
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                async () =>
                {
                    tbSkinTemperature.Text = string.Format("{0:0}", CTD.Sensors["SkinTemperature"].message.value);
                });

            //Debug.WriteLine("SkinTemperature Changed: " + string.Format("{0:0}", CTD.Sensors["SkinTemperature"].value));
        }

        private async void HeartRate_ReadingChanged(object sender, BandSensorReadingEventArgs<IBandHeartRateReading> args)
        {
            // do work when the reading changes (i.e., update a UI element)
            CTD.Sensors["HeartRate"].message.value = args.SensorReading.HeartRate;
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                async () =>
                {
                    tbHeartRate.Text = string.Format("{0:0}", CTD.Sensors["HeartRate"].message.value);
                });

            //Debug.WriteLine("HeartRate Changed: " + string.Format("{0:0}", CTD.Sensors["HeartRate"].value));
        }

        private async void Accelerometer_ReadingChanged(object sender, BandSensorReadingEventArgs<IBandAccelerometerReading> args)
        {
            // do work when the reading changes (i.e., update a UI element)
            CTD.Sensors["Acceleration"].message.value = args.SensorReading.AccelerationX;
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                async () =>
                {
                    tbAcceleration.Text = string.Format("{0:0}", CTD.Sensors["Acceleration"].message.value);
                });

            //Debug.WriteLine("HeartRate Changed: " + string.Format("{0:0}", CTD.Sensors["HeartRate"].value));
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

        private async Task connectToBand()
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

                AddBandSensor<IBandAccelerometerReading>(bandClient.SensorManager.Accelerometer,
                                    "Acceleration",
                                    "G",
                                    Accelerometer_ReadingChanged);
            }
            catch (BandException ex)
            {
                // handle a Band connection exception 
                var dialogbox = new MessageDialog("Error while connecting Band Sensors: " + ex.Message.ToString() + "\nRetrying...");
                await dialogbox.ShowAsync();
                await connectToBand();
            }
        }

        private void cbHeartRate_Checked(object sender, RoutedEventArgs e)
        {
            if (CTD != null)
                CTD.setSensorStreaming("HeartRate", true);
        }

        private void cbHeartRate_Unchecked(object sender, RoutedEventArgs e)
        {
            if (CTD != null)
                CTD.setSensorStreaming("HeartRate", false);
        }

        private void cbSkinTemperature_Checked(object sender, RoutedEventArgs e)
        {
            if (CTD != null)
                CTD.setSensorStreaming("SkinTemperature", true);
        }

        private void cbSkinTemperature_Unchecked(object sender, RoutedEventArgs e)
        {
            if (CTD != null)
                CTD.setSensorStreaming("SkinTemperature", false);
        }

        private void cbSAccelerometer_Unchecked(object sender, RoutedEventArgs e)
        {
            if (CTD != null)
                CTD.setSensorStreaming("Acceleration", false);
        }

        private void cbSAccelerometer_Checked(object sender, RoutedEventArgs e)
        {
            if (CTD != null)
                CTD.setSensorStreaming("Acceleration", true);
        }
    }
}
