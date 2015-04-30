using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Web.Http;

namespace ConnectTheDotsWPSensors
{
    public class ConnectTheDotsHelper
    {
        // App Settings variables
        public AppSettings localSettings = new AppSettings();

        public List<ConnectTheDotsSensor> sensors;

        // Http connection string, SAS tokem and client
        Uri uri;
        private string sas;
        HttpClient httpClient = new HttpClient();
        bool EventHubConnectionInitialized = false;

        public ConnectTheDotsHelper(string serviceBusNamespace = "",
            string eventHubName = "",
            string keyName = "",
            string key = "",
            string displayName = "",
            string organization = "",
            string location = "",
            List<ConnectTheDotsSensor> sensorList = null)
        {
            localSettings.ServicebusNamespace = serviceBusNamespace;
            localSettings.EventHubName = eventHubName;
            localSettings.KeyName = keyName;
            localSettings.Key = key;
            localSettings.DisplayName = displayName;
            localSettings.Organization = organization;
            localSettings.Location = location;

            sensors = sensorList;

            SaveSettings();
        }

        /// <summary>
        /// Validate the settings 
        /// </summary>
        /// <returns></returns>
        bool ValidateSettings()
        {
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
        /// Apply new settings to sensors collection
        /// </summary>
        public bool SaveSettings()
        {
            if (ValidateSettings())
            {
                ApplySettingsToSensors();
                this.InitEventHubConnection();
                return true;
            } else {
                return false;
            }
        }


        /// <summary>
        ///  Apply settings to sensors collection
        /// </summary>
        public void ApplySettingsToSensors()
        {
            foreach (ConnectTheDotsSensor sensor in sensors)
            {
                sensor.displayname = this.localSettings.DisplayName;
                sensor.location = this.localSettings.Location;
                sensor.organization = this.localSettings.Organization;
            }
        }

        private void SendAllSensorData()
        {
            foreach (ConnectTheDotsSensor sensor in sensors)
            {
                sensor.timecreated = DateTime.UtcNow.ToString("o");
                sendMessage(sensor.ToJson());
            }
        }

        public void SendSensorData(ConnectTheDotsSensor sensor)
        {
            sensor.timecreated = DateTime.UtcNow.ToString("o");
            sendMessage(sensor.ToJson());
        }

        /// <summary>
        /// Send message to Azure Event Hub using HTTP/REST API
        /// </summary>
        /// <param name="message"></param>
        public async void sendMessage(string message)
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
                        Debug.WriteLine("Failed sending message: {0}", postResult.ReasonPhrase);
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
        public bool InitEventHubConnection()
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
    }
}
