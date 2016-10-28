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
using System.Threading;
using System.Web.Services;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Collections.Generic;

namespace ConnectTheDotsWebSite
{
    public partial class Default : System.Web.UI.Page
    {
        protected string ForceSocketCloseOnUserActionsTimeout = "false";

        protected static bool IsUserAuthenticated()
        {
            var claimsPrincipal = Thread.CurrentPrincipal as ClaimsPrincipal;
            return (claimsPrincipal != null && claimsPrincipal.Identity.IsAuthenticated);
        }

        protected static bool IsUserAdmin()
        {
            var claimsPrincipal = Thread.CurrentPrincipal as ClaimsPrincipal;
            if (claimsPrincipal != null && claimsPrincipal.Identity.IsAuthenticated)
            {
                return (Global.globalSettings.AdminName == claimsPrincipal.Identity.Name);
            }
            else
                return false;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            ForceSocketCloseOnUserActionsTimeout =
                Global.globalSettings.ForceSocketCloseOnUserActionsTimeout.ToString();

            var claimsPrincipal = Thread.CurrentPrincipal as ClaimsPrincipal;
            user.InnerHtml = (IsUserAuthenticated())?(claimsPrincipal.Identity.Name + (IsUserAdmin() ? " (ADMIN)" : " (USER)")) : "User Not Authenticated";
        }

        [WebMethod]
        public static string GetDevicesList()
        {
            // Set the flag for the server to refresh the devices list from IoTHub and wait till its done
            if (Global.TriggerAndWaitDeviceListRefresh(10))
            {
                // We need to Filter the devices secret information in case the user is not an admin
                List<DeviceDetails> devicesList = Global.devicesList;
                if (!IsUserAdmin())
                {
                    foreach (DeviceDetails device in devicesList)
                    {
                        device.connectionstring = "User not allowed to access";
                    }
                }

                return JsonConvert.SerializeObject(devicesList);
            }
            return null;
        }

        [WebMethod]
        public static string AddDevice(string deviceName)
        {
            // Check if user is authorized, then create a new device
            if (!IsUserAdmin())
                return "{\"Error\": \"User not authorized to add device.\"}";

            string returnMessage;

            // Add device
            switch (Global.TriggerAndWaitAddDevice(10, deviceName))
            {
                case Helpers.IoTHubHelper.AddDeviceResult.Success:
                    Global.TriggerAndWaitDeviceListRefresh(3);
                    returnMessage  = "{\"Device\": \"" + deviceName + "\"}";
                    break;

                case Helpers.IoTHubHelper.AddDeviceResult.DeviceAlreadyExists:
                    returnMessage = "{\"Error\": \"Device already exists.\"}";
                    break;

                case Helpers.IoTHubHelper.AddDeviceResult.Unknown:
                    returnMessage = "{\"Error\": \"It's taking longer than expected to create a new device ID.\"}";
                    break;

                default:
                    returnMessage = "{\"Error\": \"An error occured when trying to add new device.\"}";
                    break;
            }
            return returnMessage;
        }

    }
}