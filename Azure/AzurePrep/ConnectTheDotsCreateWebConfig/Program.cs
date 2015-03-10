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
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Xml;

using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Management.ServiceBus;

namespace ConnectTheDotsCreateWebConfig
{
    class Program
    {
        // from cmd line
        string NamePrefix;
        string SBNamespace;
        string Location;
        string EventHubNameDevices;
        string EventHubNameAlerts;
        string StorageAccountName;
        string WebSiteDirectory;

        // from publish settings
        X509Certificate2 ManagementCertificate;
        string SubscriptionId;

        // location of other projects
        bool Transform = false;

        //--//

        ServiceBusManagementClient sbMgmt;
        string nsConnectionString;
        string ehDevicesWebSiteConnectionString;
        string ehAlertsWebSiteConnectionString;
        string storageKey;
        EventHubDescription ehDevices;
        EventHubDescription ehAlerts;
        string webConfigFile;

        public bool Run()
        {
            // Obtain management via .publishsettings file from https://manage.windowsazure.com/publishsettings/index?schemaversion=2.0
            var creds = new CertificateCloudCredentials(SubscriptionId, ManagementCertificate);

            if (!CreateWeb(creds))
            {
                return false;
            }

            #region print results

            Console.WriteLine();
            Console.WriteLine("Web.Config saved to {0}", webConfigFile);

            #endregion
            return true;
        }

        private bool CreateWeb(CertificateCloudCredentials creds)
        {
            // Create Namespace
            sbMgmt = new ServiceBusManagementClient(creds);

            var nsDescription = sbMgmt.Namespaces.GetNamespaceDescription(SBNamespace);
            nsConnectionString = nsDescription.NamespaceDescriptions.First(
                (d) => String.Equals(d.AuthorizationType, "SharedAccessAuthorization")
                ).ConnectionString;

            NamespaceManager nsManager = NamespaceManager.CreateFromConnectionString(nsConnectionString);
            ehDevices = nsManager.GetEventHub(EventHubNameDevices);
            ehAlerts = nsManager.GetEventHub(EventHubNameAlerts);

            var ehDevicesWebSiteConnectionString = new ServiceBusConnectionStringBuilder(nsConnectionString)
            {
                SharedAccessKeyName = "WebSite",
                SharedAccessKey = (ehDevices.Authorization.First((d)
                    => String.Equals(d.KeyName, "WebSite", StringComparison.InvariantCultureIgnoreCase)) as SharedAccessAuthorizationRule).PrimaryKey,
            }.ToString();

            ehAlertsWebSiteConnectionString = new ServiceBusConnectionStringBuilder(nsConnectionString)
            {
                SharedAccessKeyName = "WebSite",
                SharedAccessKey = (ehAlerts.Authorization.First((d)
                    => String.Equals(d.KeyName, "WebSite", StringComparison.InvariantCultureIgnoreCase)) as SharedAccessAuthorizationRule).PrimaryKey,
            }.ToString();
            //sbMgmt.Namespaces.Get("").Namespace.

            // Write a new web.config template file
            var doc = new XmlDocument();
            doc.PreserveWhitespace = true;

            var inputFileName = (this.Transform ? "\\web.PublishTemplate.config" : "\\web.config");
            var outputFileName = (this.Transform ? String.Format("\\web.{0}.config", NamePrefix) : "\\web.config");

            doc.Load(WebSiteDirectory + inputFileName);

            doc.SelectSingleNode("/configuration/appSettings/add[@key='Microsoft.ServiceBus.EventHubDevices']/@value").Value
                = EventHubNameDevices;
            doc.SelectSingleNode("/configuration/appSettings/add[@key='Microsoft.ServiceBus.EventHubAlerts']/@value").Value
                = EventHubNameAlerts;
            doc.SelectSingleNode("/configuration/appSettings/add[@key='Microsoft.ServiceBus.ConnectionString']/@value").Value
                = nsConnectionString;
            doc.SelectSingleNode("/configuration/appSettings/add[@key='Microsoft.ServiceBus.ConnectionStringDevices']/@value").Value
                = ehDevicesWebSiteConnectionString;
            doc.SelectSingleNode("/configuration/appSettings/add[@key='Microsoft.ServiceBus.ConnectionStringAlerts']/@value").Value
                = ehAlertsWebSiteConnectionString;
            doc.SelectSingleNode("/configuration/appSettings/add[@key='Microsoft.Storage.ConnectionString']/@value").Value =
                String.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", StorageAccountName, storageKey);

            var outputFile = System.IO.Path.GetFullPath(WebSiteDirectory + outputFileName);

            doc.Save(outputFile);

            webConfigFile = outputFile;

            return true;
        }

        static int Main(string[] args)
        {
            var p = new Program();

            bool result = p.Parse(args);

            if (result)
            {
                try
                {
                    result = p.Run();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception {0} while creating Azure resources at {1}", e.Message, e.StackTrace);
                }
            }

            return result ? 0 : 1;
        }

        private bool Parse(string[] args)
        {
            bool bParseError = false;
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].Substring(0, 1).Replace("/", "-") + args[i].Substring(1).ToLowerInvariant())
                {
                    case "-nameprefix":
                    case "-n":
                        i++;
                        if (i < args.Length)
                        {
                            NamePrefix = args[i];
                        }
                        else
                        {
                            Console.WriteLine("Error: missing NamePrefix argument");
                            bParseError = true;
                        }
                        break;
                    case "-publishsettingsfile":
                    case "-ps":
                        try
                        {
                            i++;
                            if (i >= args.Length)
                            {
                                Console.WriteLine("Error: missing NamePrefix argument");
                                bParseError = true;
                            }
                            else
                            {

                                var doc = new XmlDocument();
                                doc.Load(args[i]);
                                var certNode =
                                    doc.SelectSingleNode(
                                        "/PublishData/PublishProfile/@ManagementCertificate");
                                // Some publishsettings files (with multiple subscriptions?) have the management publisherCertificate under the Subscription
                                if (certNode == null)
                                {
                                    certNode =
                                    doc.SelectSingleNode(
                                        "/PublishData/PublishProfile/Subscription/@ManagementCertificate");
                                }
                                ManagementCertificate = new X509Certificate2(Convert.FromBase64String(certNode.Value));
                                var subNode =
                                    doc.SelectSingleNode("/PublishData/PublishProfile/Subscription/@Id");
                                SubscriptionId = subNode.Value;
                            }
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine("Error: invalid publishsettings file - {0}", exception.Message);
                            bParseError = true;
                        }
                        break;
                    case "-transform":
                        Transform = true;
                        break;
                    default:
                        Console.WriteLine("Error: unrecognized argument: {0}", args[i]);
                        bParseError = true;
                        break;
                }
                if (bParseError)
                {
                    break;
                }
            }

            if (bParseError)
            {
                Console.WriteLine("Usage: ConnectTheDotsAzurePrep -PublishSettingsFile <settingsfile> [-NamePrefix <prefix>] [-Location <location>] [-website <websitedir>]");
                return false;
            }

            if (NamePrefix == null)
            {
                NamePrefix = "IoTDemo" + Guid.NewGuid().ToString("N").GetHashCode().ToString("x");
            }
            if (Location == null)
            {
                Location = "Central US";
            }
            if (SBNamespace == null)
            {
                SBNamespace = NamePrefix + "-ns";
            }
            if (StorageAccountName == null)
            {
                StorageAccountName = NamePrefix.ToLowerInvariant() + "storage";
            }
            if (EventHubNameDevices == null)
            {
                EventHubNameDevices = "ehdevices";
            }
            if (EventHubNameAlerts == null)
            {
                EventHubNameAlerts = "ehalerts";
            }
            if (WebSiteDirectory == null)
            {
                WebSiteDirectory = "..\\..\\..\\..\\WebSite\\ConnectTheDotsWebSite"; // Default for running the tool from the bin/debug or bin/release directory (i.e within VS)
            }

            return true;
        }

    }
}
