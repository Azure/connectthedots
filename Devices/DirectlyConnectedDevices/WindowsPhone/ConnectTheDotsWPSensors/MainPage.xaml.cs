//  ---------------------------------------------------------------------------------
//  Copyright (c) Microsoft Open Technologies, Inc.  All rights reserved.
// 
//  The MIT License (MIT)
// 
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using Windows.Web.Http;
using Windows.Data.Json;
using System.Diagnostics;
using System.Net;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Devices.Sensors;
using Windows.UI.Core;
using System.Runtime.Serialization.Json;
using System.Text;
using Windows.Storage;

namespace ConnectTheDotsWPSensors
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        // App Settings variables
        SettingsFlyout SettingsPane = null;

        ConnectTheDotsHelper ctdHelper = null;
        // Hard coding guid for sensors. Not an issue for this particular application which is meant for testing and demos
        private List<ConnectTheDotsSensor> sensors = new List<ConnectTheDotsSensor> {
            new ConnectTheDotsSensor("2298a348-e2f9-4438-ab23-82a3930662ab", "Temperature", "F"),
            new ConnectTheDotsSensor("41613409-7e93-4e33-9cdd-d99eba60d646", "Humidity", "%"),
            new ConnectTheDotsSensor("2aa348bc-6984-4b86-b37e-bd69eabe8364", "Light", "Lux"),
            new ConnectTheDotsSensor("a31a645f-d431-4963-9e39-748c525b29d4", "AccelX", "g"),
            new ConnectTheDotsSensor("16a3804c-5590-401e-a239-1b529e39a545", "AccelY", "g"),
            new ConnectTheDotsSensor("6d0a055c-6a0b-46d9-86e5-8398d7b41ace", "AccelZ", "g"),
        };

        // Timing for the tasks
        private const int SimulatedDataTick = 1000; // In milliseconds
        private const int RealSensorDataTick = 500; // In milliseconds

        // Loop for reading and sending simulated data
        DispatcherTimer timer;

        // Sensors
        LightSensor light;
        Accelerometer accelerometer;

        /// <summary>
        /// Showing appsettings popup and update settings accordingly
        /// </summary>
        private void SetAppSettings()
        {
            Rect WindowBounds = Window.Current.Bounds;

            if (SettingsPane == null )
            {
                SettingsPane = new SettingsFlyout();
                SettingsPopup.Child = SettingsPane;
                SettingsPopup.Width = WindowBounds.Width;
            }
            SettingsPane.Width = SettingsPopup.Width;
            SettingsPane.Height = SettingsPopup.Height;

            SettingsPopup.IsOpen = true;
        }

        /// <summary>
        /// When appSettings popup closes, apply new settings to sensors collection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SettingsPopup_Closed(object sender, object e)
        {
            if (!ctdHelper.SaveSettings())
            {
                this.SettingsPopup.IsOpen = true;
            }
        }

        /// <summary>
        /// Initialize hardware sensors on the device
        /// </summary>
        private void InitSensors()
        {
            // Get default light sensor 
            light = LightSensor.GetDefault();
            if (light != null)
            {
                light.ReportInterval = RealSensorDataTick;
                light.ReadingChanged += light_ReadingChanged; 
            }

            // Get default acelerometer sensor 
            accelerometer = Accelerometer.GetDefault();
            if (accelerometer != null)
            {
                accelerometer.ReportInterval = RealSensorDataTick;
                accelerometer.ReadingChanged += accelerometer_ReadingChanged;
            }
        }

        /// <summary>
        /// Acceleromter reading changed. Send data to Azure Event Hub if checkbox checked in the UI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        async private void accelerometer_ReadingChanged(Accelerometer sender, AccelerometerReadingChangedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                ConnectTheDotsSensor sensor = ctdHelper.sensors.Find(item => item.measurename == "AccelX");
                if (sensor != null)
                {
                    sensor.value = args.Reading.AccelerationX;
                    sensor.timecreated = DateTime.UtcNow.ToString("o");
                    txtAccelXSensor.Text = sensor.value.ToString();
                    if (Convert.ToBoolean(chkAccelXSensor.IsChecked))
                    {
                        ctdHelper.sendMessage(sensor.ToJson());
                    }
                }

                sensor = ctdHelper.sensors.Find(item => item.measurename == "AccelY");
                if (sensor != null)
                {
                    sensor.value = args.Reading.AccelerationY;
                    sensor.timecreated = DateTime.UtcNow.ToString("o");
                    txtAccelYSensor.Text = sensor.value.ToString();
                    if (Convert.ToBoolean(chkAccelYSensor.IsChecked))
                    {
                        ctdHelper.sendMessage(sensor.ToJson());
                    }
                }

                sensor = ctdHelper.sensors.Find(item => item.measurename == "AccelZ");
                if (sensor != null)
                {
                    sensor.value = args.Reading.AccelerationZ;
                    sensor.timecreated = DateTime.UtcNow.ToString("o");
                    txtAccelZSensor.Text = sensor.value.ToString();
                    if (Convert.ToBoolean(chkAccelZSensor.IsChecked))
                    {
                        ctdHelper.sendMessage(sensor.ToJson());
                    }
                }
            });
        }

        /// <summary>
        /// Light sensor reading changed. Send data to Azure Event Hub if checkbox checked in the UI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        async private void light_ReadingChanged(LightSensor sender, LightSensorReadingChangedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                txtLightSensor.Text = args.Reading.IlluminanceInLux.ToString();
                if (Convert.ToBoolean(chkLightSensor.IsChecked))
                {
                    ConnectTheDotsSensor sensor = ctdHelper.sensors.Find(item => item.measurename == "Light");
                    if (sensor != null)
                    {
                        sensor.value = args.Reading.IlluminanceInLux;
                        sensor.timecreated = DateTime.UtcNow.ToString("o");
                        ctdHelper.sendMessage(sensor.ToJson());
                    }
                }
            });
        }

        /// <summary>
        /// Main page constructor
        /// </summary>
        public MainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
            Windows.Phone.UI.Input.HardwareButtons.BackPressed += HardwareButtons_BackPressed;
            
            // Update ScrollViewer sizew
            this.ScrollViewer.Height = Window.Current.Bounds.Height;

            ctdHelper = new ConnectTheDotsHelper(sensorList: this.sensors);

            // Get app settings
            if (!ctdHelper.localSettings.SettingsSet)
            {
                this.SetAppSettings();
            }
            else
            {
                // Setup sensor objects
                ctdHelper.ApplySettingsToSensors();

                // Initialize Event Hub connection
                if (!ctdHelper.InitEventHubConnection()) this.SetAppSettings();
            }

            // Init sensors
            this.InitSensors();
            
            // Start 1 second timer for sending simulated temp and humidity data every second
            timer = new DispatcherTimer();
            timer.Tick += Timer_tick;
            timer.Interval = TimeSpan.FromMilliseconds(SimulatedDataTick);
            timer.Start();
        }

        /// <summary>
        /// Manage backButton pressed: close appsettings panel if open, and exit app if on main page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void HardwareButtons_BackPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
            if (this.SettingsPopup.IsOpen)
            {
                this.SettingsPopup.IsOpen = false;
                e.Handled = true;
            }
            else
            {
                Application.Current.Exit();
            }
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        async protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Hide Status bar
            Windows.UI.ViewManagement.StatusBar statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
            await statusBar.HideAsync(); 
        }

        /// <summary>
        /// Timer tick for sending simulated data read from sliders on main page of the app
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_tick(object sender, object e)
        {
            try {
                    ConnectTheDotsSensor sensor = ctdHelper.sensors.Find(item => item.measurename == "Temperature");
                    if (sensor != null)
                    {
                        sensor.value = this.TemperatureSlider.Value;
                        sensor.timecreated = DateTime.UtcNow.ToString("o");
                        ctdHelper.sendMessage(sensor.ToJson());
                    }
                    sensor = ctdHelper.sensors.Find(item => item.measurename == "Humidity");
                    if (sensor != null)
                    {
                        sensor.value = this.HumiditySlider.Value;
                        sensor.timecreated = DateTime.UtcNow.ToString("o");
                        ctdHelper.sendMessage(sensor.ToJson());
                    }
            }
            catch (Exception exception)
            {
                Debug.WriteLine("Exception while sending data: {0}", exception.Message);
            }
        }

        /// <summary>
        /// When the settings button is clicked, open the appsettings popup
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.SetAppSettings();
        }
    }
}
