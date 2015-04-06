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
    ///  Class for managing app settings
    /// </summary>
    public class AppSettings
    {
        // Our settings
        ApplicationDataContainer localSettings;

        // The key names of our settings
        const string SettingsSetKeyname = "settingsset";
        const string ServicebusNamespaceKeyname = "namespace";
        const string EventHubNameKeyname = "eventhubname";
        const string KeyNameKeyname = "keyname";
        const string KeyKeyname = "key";
        const string DisplayNameKeyname = "displayname";
        const string OrganizationKeyname = "organization";
        const string LocationKeyname = "location";

        // The default value of our settings
        const bool SettingsSetDefault = false;
        const string ServicebusNamespaceDefault = "";
        const string EventHubNameDefault = "";
        const string KeyNameDefault = "";
        const string KeyDefault = "";
        const string DisplayNameDefault = "";
        const string OrganizationDefault = "";
        const string LocationDefault = "";

        /// <summary>
        /// Constructor that gets the application settings.
        /// </summary>
        public AppSettings()
        {
            // Get the settings for this application.
            localSettings = ApplicationData.Current.LocalSettings;
        }

        /// <summary>
        /// Update a setting value for our application. If the setting does not
        /// exist, then add the setting.
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool AddOrUpdateValue(string Key, Object value)
        {
            bool valueChanged = false;

            // If the key exists
            if (localSettings.Values.ContainsKey(Key))
                {
                // If the value has changed
                    if (localSettings.Values[Key] != value)
                {
                    // Store the new value
                    localSettings.Values[Key] = value;
                    valueChanged = true;
                }
            }
            // Otherwise create the key.
            else
            {
                localSettings.Values.Add(Key, value);
                valueChanged = true;
            }
            return valueChanged;
        }

        /// <summary>
        /// Get the current value of the setting, or if it is not found, set the 
        /// setting to the default setting.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public T GetValueOrDefault<T>(string Key, T defaultValue)
        {
            T value;

            // If the key exists, retrieve the value.
            if (localSettings.Values.ContainsKey(Key))
            {
                value = (T)localSettings.Values[Key];
            }
            // Otherwise, use the default value.
            else
            {
                value = defaultValue;
            }
            return value;
        }

        /// <summary>
        /// Save the settings.
        /// </summary>
        public void Save()
        {
            // keeping the below in case we want to use this code on a Windows Phone 8 device.
            // With universal Windows Apps, this is no longer necessary as settings are saved automatically
//            settings.Save();
        }


        /// <summary>
        /// Property to get and set a Username Setting Key.
        /// </summary>
        public bool SettingsSet
        {
            get
            {
                return GetValueOrDefault<bool>(SettingsSetKeyname, SettingsSetDefault);
            }
            set
            {
                if (AddOrUpdateValue(SettingsSetKeyname, value))
                {
                    Save();
                }
            }
        }

        /// <summary>
        /// Property to get and set a Username Setting Key.
        /// </summary>
        public string ServicebusNamespace
        {
            get
            {
                return GetValueOrDefault<string>(ServicebusNamespaceKeyname, ServicebusNamespaceDefault);
            }
            set
            {
                if (AddOrUpdateValue(ServicebusNamespaceKeyname, value))
                {
                    Save();
                }
            }
        }

        /// <summary>
        /// Property to get and set a Username Setting Key.
        /// </summary>
        public string EventHubName
        {
            get
            {
                return GetValueOrDefault<string>(EventHubNameKeyname, EventHubNameDefault);
            }
            set
            {
                if (AddOrUpdateValue(EventHubNameKeyname, value))
                {
                    Save();
                }
            }
        }
        /// <summary>
        /// Property to get and set a Username Setting Key.
        /// </summary>
        public string KeyName
        {
            get
            {
                return GetValueOrDefault<string>(KeyNameKeyname, KeyNameDefault);
            }
            set
            {
                if (AddOrUpdateValue(KeyNameKeyname, value))
                {
                    Save();
                }
            }
        }
        /// <summary>
        /// Property to get and set a Username Setting Key.
        /// </summary>
        public string Key
        {
            get
            {
                return GetValueOrDefault<string>(KeyKeyname, KeyDefault);
            }
            set
            {
                if (AddOrUpdateValue(KeyKeyname, value))
                {
                    Save();
                }
            }
        }
        /// <summary>
        /// Property to get and set a Username Setting Key.
        /// </summary>
        public string DisplayName
        {
            get
            {
                return GetValueOrDefault<string>(DisplayNameKeyname, DisplayNameDefault);
            }
            set
            {
                if (AddOrUpdateValue(DisplayNameKeyname, value))
                {
                    Save();
                }
            }
        }
        /// <summary>
        /// Property to get and set a Username Setting Key.
        /// </summary>
        public string Organization
        {
            get
            {
                return GetValueOrDefault<string>(OrganizationKeyname, OrganizationDefault);
            }
            set
            {
                if (AddOrUpdateValue(OrganizationKeyname, value))
                {
                    Save();
                }
            }
        }
        /// <summary>
        /// Property to get and set a Username Setting Key.
        /// </summary>
        public string Location
        {
            get
            {
                return GetValueOrDefault<string>(LocationKeyname, LocationDefault);
            }
            set
            {
                if (AddOrUpdateValue(LocationKeyname, value))
                {
                    Save();
                }
            }
        }

    }

    /// <summary>
    /// Class to manage sensor data and attributes 
    /// </summary>
    public class ConnecTheDotsSensor
    {
        public string guid { get; set; }
        public string displayname { get; set; }
        public string organization { get; set; }
        public string location { get; set; }
        public string measurename { get; set; }
        public string unitofmeasure { get; set; }
        public string timecreated { get; set; }
        public double value { get; set; }

        /// <summary>
        /// Default parameterless constructor needed for serialization of the objects of this class
        /// </summary>
        public ConnecTheDotsSensor()
        { 
        }

        /// <summary>
        /// Construtor taking parameters guid, measurename and unitofmeasure
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="measurename"></param>
        /// <param name="unitofmeasure"></param>
        public ConnecTheDotsSensor(string guid, string measurename, string unitofmeasure)
        {
            this.guid = guid;
            this.measurename = measurename;
            this.unitofmeasure = unitofmeasure;
        }

        /// <summary>
        /// ToJson function is used to convert sensor data into a JSON string to be sent to Azure Event Hub
        /// </summary>
        /// <returns>JSon String containing all info for sensor data</returns>
        public string ToJson()
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(ConnecTheDotsSensor));
            MemoryStream ms = new MemoryStream();
            ser.WriteObject(ms, this);
            string json = Encoding.UTF8.GetString(ms.ToArray(), 0, (int)ms.Length);

            return json;
        }
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        // App Settings variables
        AppSettings localSettings = new AppSettings();
        SettingsFlyout SettingsPane = null;

        // Hard coding guid for sensors. Not an issue for this particular application which is meant for testing and demos
        private List<ConnecTheDotsSensor> sensors = new List<ConnecTheDotsSensor> {
            new ConnecTheDotsSensor("2298a348-e2f9-4438-ab23-82a3930662ab", "Temperature", "F"),
            new ConnecTheDotsSensor("41613409-7e93-4e33-9cdd-d99eba60d646", "Humidity", "%"),
            new ConnecTheDotsSensor("2aa348bc-6984-4b86-b37e-bd69eabe8364", "Light", "Lux"),
            new ConnecTheDotsSensor("a31a645f-d431-4963-9e39-748c525b29d4", "AccelX", "g"),
            new ConnecTheDotsSensor("16a3804c-5590-401e-a239-1b529e39a545", "AccelY", "g"),
            new ConnecTheDotsSensor("6d0a055c-6a0b-46d9-86e5-8398d7b41ace", "AccelZ", "g"),
        };

        // Timing for the tasks
        private const int SimulatedDataTick = 1000; // In milliseconds
        private const int RealSensorDataTick = 500; // In milliseconds

        // Http connection string, SAS tokem and client
        Uri uri;
        private string sas;
        HttpClient httpClient = new HttpClient();
        bool EventHubConnectionInitialized = false;

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
        /// Validate the settings 
        /// </summary>
        /// <returns></returns>
        bool ValidateSettings()
        {
            // TODO: implement better settings validation here
            if ((localSettings.ServicebusNamespace == "") ||
                (localSettings.EventHubName == "") ||
                (localSettings.KeyName == "") ||
                (localSettings.Key == "") ||
                (localSettings.DisplayName == "") ||
                (localSettings.Organization == "") ||
                (localSettings.Location == ""))
            {
                this.localSettings.SettingsSet = false;
                return false;
            }

            this.localSettings.SettingsSet = true;
            return true;

        }

        /// <summary>
        /// When appSettings popup closes, apply new settings to sensors collection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SettingsPopup_Closed(object sender, object e)
        {
            if (ValidateSettings())
            {
                ApplySettingsToSensors();
                this.InitEventHubConnection();
            }
            else
            {
                this.SettingsPopup.IsOpen = true;
            }
        }

        /// <summary>
        ///  Apply settings to sensors collection
        /// </summary>
        private void ApplySettingsToSensors()
        {
            foreach (ConnecTheDotsSensor sensor in sensors)
            {
                sensor.displayname = this.localSettings.DisplayName;
                sensor.location = this.localSettings.Location;
                sensor.organization = this.localSettings.Organization;
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
                ConnecTheDotsSensor sensor = sensors.Find(item => item.measurename == "AccelX");
                if (sensor != null)
                {
                    sensor.value = args.Reading.AccelerationX;
                    sensor.timecreated = DateTime.UtcNow.ToString("o");
                    txtAccelXSensor.Text = sensor.value.ToString();
                    if (Convert.ToBoolean(chkAccelXSensor.IsChecked))
                    {
                        sendMessage(sensor.ToJson());
                    }
                }

                sensor = sensors.Find(item => item.measurename == "AccelY");
                if (sensor != null)
                {
                    sensor.value = args.Reading.AccelerationY;
                    sensor.timecreated = DateTime.UtcNow.ToString("o");
                    txtAccelYSensor.Text = sensor.value.ToString();
                    if (Convert.ToBoolean(chkAccelYSensor.IsChecked))
                    {
                        sendMessage(sensor.ToJson());
                    }
                }

                sensor = sensors.Find(item => item.measurename == "AccelZ");
                if (sensor != null)
                {
                    sensor.value = args.Reading.AccelerationZ;
                    sensor.timecreated = DateTime.UtcNow.ToString("o");
                    txtAccelZSensor.Text = sensor.value.ToString();
                    if (Convert.ToBoolean(chkAccelZSensor.IsChecked))
                    {
                        sendMessage(sensor.ToJson());
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
                    ConnecTheDotsSensor sensor = sensors.Find(item => item.measurename == "Light");
                    if (sensor != null)
                    {
                        sensor.value = args.Reading.IlluminanceInLux;
                        sensor.timecreated = DateTime.UtcNow.ToString("o");
                        sendMessage(sensor.ToJson());
                    }
                }
            });
        }

        /// <summary>
        /// Send message to Azure Event Hub using HTTP/REST API
        /// </summary>
        /// <param name="message"></param>
        private async void sendMessage(string message)
        {
            if (this.EventHubConnectionInitialized)
            {
                try
                {
                    HttpStringContent content = new HttpStringContent(message, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/json");
                    HttpResponseMessage postResult = await httpClient.PostAsync(uri, content);

                    if (postResult.IsSuccessStatusCode)
                    {
                        Debug.WriteLine("Message Sent: {0}", content);
                    }
                    else
                    {
                        Debug.WriteLine("Failed sending messge: {0}", postResult.ReasonPhrase);
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Exception when sending message:" + e.Message);
                }
            }
        }

        /// <summary>
        /// Helper function to get SAS token for connecting to Azure Event Hub
        /// </summary>
        /// <returns></returns>
        private string SASTokenHelper()
        {
            int expiry = (int)DateTime.UtcNow.AddMinutes(20).Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            string stringToSign = WebUtility.UrlEncode(this.uri.ToString()) + "\n" + expiry.ToString();
            string signature = HmacSha256(this.localSettings.Key.ToString(), stringToSign);
            string token = String.Format("sr={0}&sig={1}&se={2}&skn={3}", WebUtility.UrlEncode(this.uri.ToString()), WebUtility.UrlEncode(signature), expiry, this.localSettings.KeyName.ToString());

            return token;
        }

        /// <summary>
        /// Because Windows.Security.Cryptography.Core.MacAlgorithmNames.HmacSha256 doesn't
        /// exist in WP8.1 context we need to do another implementation
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public string HmacSha256(string key, string value)
        {
            var keyStrm = CryptographicBuffer.ConvertStringToBinary(key, BinaryStringEncoding.Utf8);
            var valueStrm = CryptographicBuffer.ConvertStringToBinary(value, BinaryStringEncoding.Utf8);

            var objMacProv = MacAlgorithmProvider.OpenAlgorithm(MacAlgorithmNames.HmacSha256);
            var hash = objMacProv.CreateHash(keyStrm);
            hash.Append(valueStrm);

            return CryptographicBuffer.EncodeToBase64String(hash.GetValueAndReset());
        }
        
        /// <summary>
        /// Initialize Event Hub connection
        /// </summary>
        private bool InitEventHubConnection()
        {
            try
            {
                this.uri = new Uri("https://" + this.localSettings.ServicebusNamespace +
                              ".servicebus.windows.net/" + this.localSettings.EventHubName +
                              "/publishers/" + this.localSettings.DisplayName + "/messages");
                this.sas = SASTokenHelper();
                this.httpClient.DefaultRequestHeaders.Authorization = new Windows.Web.Http.Headers.HttpCredentialsHeaderValue("SharedAccessSignature", sas);
                this.EventHubConnectionInitialized = true;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
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

            // Get app settings
            if (!this.localSettings.SettingsSet)
            {
                this.SetAppSettings();
            }
            else
            {
                // Setup sensor objects
                ApplySettingsToSensors();

                // Initialize Event Hub connection
                if (!this.InitEventHubConnection()) this.SetAppSettings();
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
                    ConnecTheDotsSensor sensor = sensors.Find(item => item.measurename == "Temperature");
                    if (sensor != null)
                    {
                        sensor.value = this.TemperatureSlider.Value;
                        sensor.timecreated = DateTime.UtcNow.ToString("o");
                        sendMessage(sensor.ToJson());
                    }
                    sensor = sensors.Find(item => item.measurename == "Humidity");
                    if (sensor != null)
                    {
                        sensor.value = this.HumiditySlider.Value;
                        sensor.timecreated = DateTime.UtcNow.ToString("o");
                        sendMessage(sensor.ToJson());
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
