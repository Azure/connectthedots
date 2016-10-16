using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Azure.Devices;
using System.Text;
using System.Threading.Tasks;

namespace ConnectTheDotsWebSite.Helpers
{
    public class IoTHubHelper
    {
        public static string cs;
        private static ServiceClient serviceClient = null;
        private static RegistryManager manager = null;

        public IoTHubHelper(string connectionString)
        {
            cs = connectionString;
            serviceClient = ServiceClient.CreateFromConnectionString(cs, TransportType.Amqp);
            manager = RegistryManager.CreateFromConnectionString(cs);
        }

        public void SendMessage(string deviceId, string messageText)
        {
            var message = new Message(Encoding.ASCII.GetBytes(messageText));
            if (serviceClient!=null) serviceClient.SendAsync(deviceId, message).Wait();
        }

        // public static void AddDevice(string deviceId)
        //{
        //    if (manager != null)           
        //        manager.AddDeviceAsync(new Device(deviceId));
        //}

        //public static void RemoveDevice(string deviceId)
        //{
        //    if (manager != null)
        //        manager.RemoveDeviceAsync(new Device(deviceId));
        //}

        public async Task<List<KeyValuePair<string, string>>> ListDevices(int count)
        {
            IEnumerable<Device> devices;
            List<KeyValuePair<string, string>> result = null;

            if (manager != null)
            {
                devices = await manager.GetDevicesAsync(count);
                if (devices.Count<Device>() > 0)
                {
                    result = new List<KeyValuePair<string, string>>();
                    foreach (Device device in devices)
                    {
                        result.Add(new KeyValuePair<string, string>(device.Id, device.Authentication.SymmetricKey.PrimaryKey));
                    }
                }
            }
            return result;
        }

    }
}