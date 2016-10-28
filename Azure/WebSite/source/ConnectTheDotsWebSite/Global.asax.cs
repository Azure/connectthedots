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
using System.Diagnostics;
using System.Web.Http;
using System.Web.Routing;
using Microsoft.ServiceBus.Messaging;
using Microsoft.Azure;

using ConnectTheDotsWebSite.Helpers;
using System.Threading;

namespace ConnectTheDotsWebSite
{
    public struct EventHubSettings
    {
        public string name { get; set; }
        public string connectionString { get; set; }
        public string consumerGroup { get; set; }
        public EventProcessorHost processorHost { get; set; }
        public EventProcessorOptions processorHostOptions { get; set; }
        public EventHubClient client { get; set; }
        //public NamespaceManager namespaceManager { get; set; }
        public string storageConnectionString { get; set; }
    }

    public struct GlobalSettings
    {
        public bool ForceSocketCloseOnUserActionsTimeout { get; set; }
        public bool RefreshDevicesList { get; set; }
        public bool DevicesListRefreshed { get; set; }
        public string AdminName { get; set; }
        public bool AddDevice { get; set; }
        public bool DeviceAdded { get; set; }
        public IoTHubHelper.AddDeviceResult DeviceAddedResult { get; set; }
        public string NewDeviceName { get; set; }
    }

    public class DeviceDetails
    {
        public string guid { get; set; }
        public string displayname { get; set; }
        public string location { get; set; }
        public string ipaddress { get; set; }
        public string connectionstring { get; set; }

        public DeviceDetails(IDictionary<string, object> deviceInfo)
        {
            if (deviceInfo.ContainsKey("guid")) guid = deviceInfo["guid"].ToString();
            if (deviceInfo.ContainsKey("displayname")) displayname = deviceInfo["displayname"].ToString();
            if (deviceInfo.ContainsKey("location")) location = deviceInfo["location"].ToString();
            if (deviceInfo.ContainsKey("ipaddress")) ipaddress = deviceInfo["ipaddress"].ToString();
            if (deviceInfo.ContainsKey("connectionstring")) connectionstring = deviceInfo["connectionstring"].ToString();
        }

    }

    public class Global : System.Web.HttpApplication
    {
        EventHubSettings eventHubDevicesSettings;
        EventHubSettings eventHubAlertsSettings;

        public static GlobalSettings globalSettings = new GlobalSettings() { ForceSocketCloseOnUserActionsTimeout = true, DevicesListRefreshed = false, RefreshDevicesList = true, AddDevice = false, DeviceAdded=false  };

        public static List<DeviceDetails> devicesList = new List<DeviceDetails>();
        private static System.Timers.Timer pingIoTHubTimer;

        public static void AddToDeviceList(IDictionary<string, object> deviceInfo)
        {
            // if the passed Dictionnary doesn't contain a guid key, there is nothing for us to do here...
            if (!deviceInfo.ContainsKey("guid")) return;

            var device = devicesList.Find(item => item.guid == deviceInfo["guid"].ToString());
            if (device != null)
            {
                // Device exists, update its fields
                if (deviceInfo.ContainsKey("connectionstring"))
                {
                    // If the updateConnectionString is set we only need/want to update the connection string...
                    device.connectionstring = deviceInfo["connectionstring"].ToString();
                }
                else {
                    // otherwise we will update the data for the device
                    if (deviceInfo.ContainsKey("displayname"))
                    {
                        device.displayname = deviceInfo["displayname"].ToString();
                    }
                    if (deviceInfo.ContainsKey("location"))
                    {
                        device.location = deviceInfo["location"].ToString();
                    }
                    if (deviceInfo.ContainsKey("ipaddress"))
                    {
                        device.ipaddress = deviceInfo["ipaddress"].ToString();
                    }
                }
            }
            else
            {
                devicesList.Add(new DeviceDetails(deviceInfo));
            }
        }

        private static void UpdateDeviceListFromIoTHub()
        {
            List<IDictionary<string, object>> devices = IoTHubHelper.ListDevices(100);
            if (devices != null)
            {
                foreach (IDictionary<string, object> device in devices)
                {
                    AddToDeviceList(device);
                }

                // Clean up the list of devices removing devices that are no longer provisionned in IoT Hub
                devicesList.RemoveAll(device => devices.Find(item => item["guid"].ToString() == device.guid) == null);

                Global.globalSettings.DevicesListRefreshed = true;
            }
        }

        public static bool TriggerAndWaitDeviceListRefresh(int timeout)
        {
            // Set the flag for the server to refresh the devices list from IoTHub and wait till its done
            Global.globalSettings.DevicesListRefreshed = false;
            Global.globalSettings.RefreshDevicesList = true;

            // Init counter (timeout is expressed in seconds)
            int tick = 0;

            // Wait till list is refreshed or till timeout is hit
            while ((!Global.globalSettings.DevicesListRefreshed) && (tick++<timeout)) Thread.Sleep(1000);

            return Global.globalSettings.DevicesListRefreshed;
        }

        public static IoTHubHelper.AddDeviceResult TriggerAndWaitAddDevice(int timeout, string deviceName)
        {
            // Set the flag for the server to refresh the devices list from IoTHub and wait till its done
            Global.globalSettings.DeviceAdded = false;
            Global.globalSettings.NewDeviceName = deviceName; 
            Global.globalSettings.AddDevice = true;

            // Init counter (timeout is expressed in seconds)
            int tick = 0;

            // Wait till list is refreshed or till timeout is hit
            while ((!Global.globalSettings.DeviceAdded) && (tick++ < timeout)) Thread.Sleep(1000);

            return Global.globalSettings.DeviceAddedResult;
        }

        protected void Application_Start(Object sender, EventArgs e)
        {

            // Set up a route for WebSocket requests
            RouteTable.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
                
            );

            // Read connection strings and Event Hubs names from app.config file
            GetAppSettings();

            // Create EventProcessorHost clients
            CreateEventProcessorHostClient(ref eventHubAlertsSettings);
            CreateEventProcessorHostClient(ref eventHubDevicesSettings);

            // Setup a timer to ping IoTHub for list of devices every second (will effectively ping IoTHub if flag refreshDevicesList is true)
            pingIoTHubTimer = new System.Timers.Timer(1000);
            pingIoTHubTimer.Elapsed += PingIoTHubTimer_Elapsed;
            pingIoTHubTimer.Elapsed += (Object source, System.Timers.ElapsedEventArgs args) => { if (Global.globalSettings.RefreshDevicesList) { Global.globalSettings.RefreshDevicesList = false; Global.UpdateDeviceListFromIoTHub(); }  };
            pingIoTHubTimer.Enabled = true;
        }

        private void PingIoTHubTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (Global.globalSettings.RefreshDevicesList)
            {
                Global.globalSettings.RefreshDevicesList = false;
                Global.UpdateDeviceListFromIoTHub();
            }

            if (Global.globalSettings.AddDevice)
            {
                Global.globalSettings.AddDevice = false;

                Global.globalSettings.DeviceAddedResult = IoTHubHelper.AddDevice(Global.globalSettings.NewDeviceName);
                Global.globalSettings.DeviceAdded = true;
            }
        }

        protected void Application_End(Object sender, EventArgs e)
        {
            Trace.TraceInformation("Unregistering EventProcessorHosts");
            if (eventHubDevicesSettings.processorHost!=null)
                eventHubDevicesSettings.processorHost.UnregisterEventProcessorAsync().Wait();
            if (eventHubAlertsSettings.processorHost != null)
                eventHubAlertsSettings.processorHost.UnregisterEventProcessorAsync().Wait();
        }
        
        private void CreateEventProcessorHostClient(ref EventHubSettings eventHubSettings)
        {
            Trace.TraceInformation("Creating EventProcessorHost: {0}, {1}, {2}", this.Server.MachineName, eventHubSettings.name, eventHubSettings.consumerGroup);
            try
            {
                eventHubSettings.client = EventHubClient.CreateFromConnectionString(eventHubSettings.connectionString,
                                                                                eventHubSettings.name);
            }
            catch (Exception ex)
            {
                // Error happened while trying to delete old ConsumerGroups.
                Debug.Print("Error happened while creating the eventhub client: " + ex.Message);
            }

            try
            {
                //eventHubSettings.processorHost = new EventProcessorHost(this.Server.MachineName,
                //    eventHubSettings.client.Path,
                //    eventHubSettings.consumerGroup.ToLowerInvariant(),
                //    eventHubSettings.connectionString,
                //    eventHubSettings.storageConnectionString);
                eventHubSettings.processorHost = new EventProcessorHost(this.Server.MachineName,
                    eventHubSettings.name,
                    eventHubSettings.consumerGroup,
                    eventHubSettings.connectionString,
                    eventHubSettings.storageConnectionString
                    );

                eventHubSettings.processorHostOptions = new EventProcessorOptions();
                eventHubSettings.processorHostOptions.InitialOffsetProvider = (partitionId) => DateTime.UtcNow;
                eventHubSettings.processorHostOptions.ExceptionReceived += WebSocketEventProcessor.ExceptionReceived;

                Trace.TraceInformation("Registering EventProcessor for " + eventHubSettings.name);
                eventHubSettings.processorHost.RegisterEventProcessorAsync<WebSocketEventProcessor>(eventHubSettings.processorHostOptions).Wait();
                //eventHubSettings.processorHost.RegisterEventProcessorAsync<WebSocketEventProcessor>().Wait();
            }
            catch (Exception e)
            {
                Debug.Print("Error happened while trying to connect Event Hub: " + e.ToString());
            }

        }

        private void GetAppSettings()
        {
            try
            {
                globalSettings.ForceSocketCloseOnUserActionsTimeout =
                    CloudConfigurationManager.GetSetting("ForceSocketCloseOnUserActionsTimeout") == "true";

                // Read settings for Devices Event Hub (IoTHub event hub compatible endpoint)
                eventHubDevicesSettings.name = CloudConfigurationManager.GetSetting("Azure.IoT.IoTHub.EventHub.Name");
                eventHubDevicesSettings.connectionString = CloudConfigurationManager.GetSetting("Azure.IoT.IoTHub.EventHub.ConnectionString");
                eventHubDevicesSettings.consumerGroup = CloudConfigurationManager.GetSetting("Azure.IoT.IoTHub.EventHub.ConsumerGroup");
                eventHubDevicesSettings.storageConnectionString = CloudConfigurationManager.GetSetting("Azure.Storage.ConnectionString");
                // eventHubDevicesSettings.namespaceManager = NamespaceManager.CreateFromConnectionString(CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString"));

                // Read settings for Alerts Event Hub
                eventHubAlertsSettings.name = CloudConfigurationManager.GetSetting("Azure.ServiceBus.EventHub.Name");
                eventHubAlertsSettings.connectionString = CloudConfigurationManager.GetSetting("Azure.ServiceBus.EventHub.ConnectionString");
                eventHubAlertsSettings.consumerGroup = CloudConfigurationManager.GetSetting("Azure.ServiceBus.EventHub.ConsumerGroup");
                eventHubAlertsSettings.storageConnectionString = CloudConfigurationManager.GetSetting("Azure.Storage.ConnectionString");
                //eventHubAlertsSettings.namespaceManager = NamespaceManager.CreateFromConnectionString(CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString"));

                // Read other settings
                globalSettings.AdminName = CloudConfigurationManager.GetSetting("AdminName");
            }
            catch (Exception)
            {
                // TODO : display error on site
            }

        }

    }
}