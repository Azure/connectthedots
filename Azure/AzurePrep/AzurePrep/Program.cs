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

namespace Microsoft.ConnectTheDots.CloudDeploy.AzurePrep
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Xml;
    using System.Net;
    using System.IO;
    using Newtonsoft.Json;
    using Microsoft.ConnectTheDots.CloudDeploy.Common;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.Management.Storage;
    using Microsoft.WindowsAzure.Management.Storage.Models;
    using Microsoft.WindowsAzure.Management.ServiceBus;
    using Microsoft.WindowsAzure.Management.ServiceBus.Models;

    //--//

    class Program
    {
        internal class AzurePrepInputs
        {
            public string NamePrefix;
            public string SBNamespace;
            public string Location;
            public string EventHubNameDevices;
            public string EventHubNameAlerts;
            public string StorageAccountName;
            public SubscriptionCloudCredentials Credentials;
        }

        internal class AzurePrepOutputs
        {
            public string SBNamespace;
            public string nsConnectionString;
            public EventHubDescription ehDevices = null;
            public EventHubDescription ehAlerts = null;
        }

        //--//

        private static readonly LogBuffer _ConsoleBuffer = new LogBuffer(
            ( m ) => {
                Console.WriteLine( m );
            }
        );

        //--//

        // from publish settings
        X509Certificate2 ManagementCertificate;
        string SubscriptionId;

        //--//

#if AZURESTREAMANALYTICS
        string StreamAnalyticsGroup;
        string JobAggregates;
        string JobAlerts;
#endif

        public bool GetInputs( out AzurePrepInputs result )
        {
            result = new AzurePrepInputs( );
            result.Credentials = AzureConsoleHelper.GetUserSubscriptionCredentials( );
            if( result.Credentials == null )
            {
                result = null;
                return false;
            }

            for( ;; )
            {
                Console.WriteLine("Enter a name for Service Bus Namespace (only letters and digits, less than 17 chars long)." );
                Console.WriteLine("(Note that fully qualified path may also be subject to further length restrictions.)");
                result.NamePrefix = Console.ReadLine( );
                if( string.IsNullOrEmpty(result.NamePrefix) || !CheckNamePrefix( result.NamePrefix ) )
                {
                    Console.WriteLine( "Namespace prefix should contain only letters and digits and have length less than 17." );
                    continue;
                }
                if (ConsoleHelper.Confirm("Are you sure you want to create a namespace called " + result.NamePrefix + "?"))
                {
                    break;
                }
            }
            
            if( string.IsNullOrEmpty( result.NamePrefix ) )
            {
                result = null;
                return false;
            }

            result.Location = SelectRegion( result );

            result.SBNamespace = result.NamePrefix + "-ns";
            result.StorageAccountName = result.NamePrefix.ToLowerInvariant( ) + "storage";

            result.EventHubNameDevices = "ehdevices";
            result.EventHubNameAlerts = "ehalerts";

#if AZURESTREAMANALYTICS
            StreamAnalyticsGroup = NamePrefix + "-StreamAnalytics";
            JobAggregates = NamePrefix + "-aggregates";
            JobAlerts = NamePrefix + "-alerts";
#endif
            return true;
        }

        public bool Run( )
        {
            AzurePrepInputs inputs;
            if( !GetInputs( out inputs ) )
            {
                Console.WriteLine( "Error while getting inputs." );
                Console.WriteLine( "Press Enter to continue..." );
                Console.ReadLine( );
                return false;
            }

            AzurePrepOutputs createResults = CreateEventHub( inputs );
            if( createResults == null )
            {
                Console.WriteLine( "Error while creating Event Hubs." );
                Console.WriteLine( "Press Enter to continue..." );
                Console.ReadLine();
                return false;
            }

            #region print results

            _ConsoleBuffer.Add( "" );
            _ConsoleBuffer.Add( "Service Bus management connection string (i.e. for use in Service Bus Explorer):" );
            _ConsoleBuffer.Add( createResults.nsConnectionString );
            _ConsoleBuffer.Add( "" );
            _ConsoleBuffer.Add( "Device AMQP address strings (for Raspberry PI/devices):" );

            for ( int i = 1; i <= 4; i++ )
            {
                var deviceKeyName = String.Format( "D{0}", i );
                var deviceKey = ( createResults.ehDevices.Authorization.First( ( d )
                        => String.Equals( d.KeyName, deviceKeyName, StringComparison.InvariantCultureIgnoreCase ) ) as SharedAccessAuthorizationRule ).PrimaryKey;

                _ConsoleBuffer.Add( string.Format( "amqps://{0}:{1}@{2}.servicebus.windows.net",
                    deviceKeyName, Uri.EscapeDataString(deviceKey), createResults.SBNamespace ) );
            }

            Console.WriteLine( "" );
            Console.WriteLine( "" );

            string fileName = createResults.SBNamespace + DateTime.UtcNow.ToString( "_d_MMM_h_mm" ) + ".log";
            string filePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string fileFullName = filePath + @"\" + fileName;
            if( _ConsoleBuffer.FlushToFile( fileFullName ) )
            {
                Console.WriteLine( "Output was saved to your desktop, at " + fileFullName + " file." );
            }

            Console.WriteLine( "Please hit enter to close." );
            Console.ReadLine( );

            #endregion

#if AZURESTREAMANALYTICS
            // Create StreamAnalyticsJobs + inputs + outputs + enter keys

            // Untested code. May require AAD authentication, no support for management cert?

            // Create Resource Group for the Stream Analytics jobs
            var groupCreateRequest = WebRequest.Create(String.Format("https://management.azure.com/subscriptions/{0}/resourcegroups/{1}?api-version=2014-04-01-preview",
                SubscriptionId, StreamAnalyticsGroup)) as HttpWebRequest;

            groupCreateRequest.ClientCertificates.Add(creds.ManagementCertificate);
            groupCreateRequest.ContentType = "application/json";
            groupCreateRequest.Method = "PUT";
            groupCreateRequest.KeepAlive = true;

            var bytesGroup = Encoding.UTF8.GetBytes("{\"location\":\"Central US\"}");
            groupCreateRequest.ContentLength = bytesGroup.Length;
            groupCreateRequest.GetRequestStream().Write(bytesGroup, 0, bytesGroup.Length);

            var groupCreateResponse = groupCreateRequest.GetResponse();

            //var streamMgmt = new ManagementClient(creds); //, new Uri("https://management.azure.com"));
            //HttpClient client = streamMgmt.HttpClient;
            
            var createJob = new StreamAnalyticsJob()
            {
                location = Location,
                inputs = new List<StreamAnalyticsEntity> 
                {
                    new StreamAnalyticsEntity 
                    {
                        name = "devicesInput",
                        properties = new Dictionary<string,object>
                        {
                            { "type" , "stream" },
                            { "serialization" , new Dictionary<string,object>
                                {
                                    { "type", "JSON"},
                                    { "properties", new Dictionary<string, object>
                                        {
                                            { "encoding", "UTF8"},
                                        }
                                    }
                                }
                            },
                            { "datasource", new Dictionary<string,object>
                                {
                                    { "type", "Microsoft.ServiceBus/EventHub" },
                                    { "properties", new Dictionary<string,object>
                                        {
                                            { "eventHubNamespace", Namespace },
                                            { "eventHubName", EventHubDevices },
                                            { "sharedAccessPolicyName", "StreamingAnalytics" },
                                            { "sharedAccessPolicyKey", 
                                                (ehDevices.Authorization.First( (d) 
                                                    => String.Equals(d.KeyName, "StreamingAnalytics", StringComparison.InvariantCultureIgnoreCase)) as SharedAccessAuthorizationRule).PrimaryKey },
                                        }
                                    }
                                }
                             }
                        },
                    },
                },
                transformation = new StreamAnalyticsEntity()
                {
                    name = "Aggregates",
                    properties = new Dictionary<string,object>
                    {
                        { "streamingUnits", 1 },
                        { "query" , "select * from devicesInput" },
                    }
                },
                outputs = new List<StreamAnalyticsEntity> 
                {
                    new StreamAnalyticsEntity 
                    {
                        name = "output",
                        properties = new Dictionary<string,object>
                        {
                            { "datasource", new Dictionary<string,object>
                                {
                                    { "type", "Microsoft.ServiceBus/EventHub" },
                                    { "properties", new Dictionary<string,object>
                                        {
                                            { "eventHubNamespace", Namespace },
                                            { "eventHubName", EventHubAlerts },
                                            { "sharedAccessPolicyName", "StreamingAnalytics" },
                                            { "sharedAccessPolicyKey", 
                                                (ehAlerts.Authorization.First( (d) => String.Equals(d.KeyName, "StreamingAnalytics", StringComparison.InvariantCultureIgnoreCase)) as SharedAccessAuthorizationRule).PrimaryKey },
                                        }
                                    }
                                }
                            },
                            { "serialization" , new Dictionary<string,object>
                                {
                                    { "type", "JSON"},
                                    { "properties", new Dictionary<string, object>
                                        {
                                            { "encoding", "UTF8"},
                                        }
                                    }
                                }
                            },
                        },
                    },
                }
            };



            var jobCreateRequest = WebRequest.Create(String.Format("https://management.azure.com/subscriptions/{0}/resourcegroups/{1}/Microsoft.StreamAnalytics/streamingjobs/{2}?api-version=2014-10-01",
                SubscriptionId, StreamAnalyticsGroup, JobAggregates)) as HttpWebRequest;

            jobCreateRequest.ClientCertificates.Add(creds.ManagementCertificate);
            jobCreateRequest.ContentType = "application/json";
            jobCreateRequest.Method = "PUT";
            jobCreateRequest.KeepAlive = true;

            var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(createJob));
            jobCreateRequest.ContentLength = bytes.Length;
            jobCreateRequest.GetRequestStream().Write(bytes, 0, bytes.Length);

            var jobCreateResponse = jobCreateRequest.GetResponse();

            //var jobCreateTask = streamMgmt.HttpClient.PutAsync(
            //    String.Format("https://management.azure.com/subscriptions/{0}/resourcegroups/{1}/Microsoft.StreamAnalytics/streamingjobs/{2}?api-version=2014-10-01",
            //    SubscriptionId, StreamAnalyticsGroup, JobAggregates),
            //    new StringContent(JsonConvert.SerializeObject(createJob)));
            //jobCreateTask.Wait();
            //var jobCreateResponse = jobCreateTask.Result;
#endif
            return true;
        }

        private bool CheckNamePrefix( string namePrefix )
        {
            //namePrefix length should be less than 17 characters (storage account names must be < 24 characters and we add "storage" to the end)
            if( namePrefix.Length >= 17 )
            {
                return false;
            }
            foreach( char c in namePrefix )
            {
                if( !char.IsLetterOrDigit( c ) )
                {
                    return false;
                }
            }
            return true;
        }

        private string SelectRegion( AzurePrepInputs inputs )
        {
            Console.WriteLine( "Retrieving a list of Locations..." );
            string[] regions = AzureProvider.GetRegions( inputs.Credentials );
            int regionsCount = regions.Length;

            Console.WriteLine( "Available locations: " );

            for( int currentRegion = 1; currentRegion <= regionsCount; ++currentRegion )
            {
                Console.WriteLine( currentRegion + ": " + regions[ currentRegion - 1 ] );
            }

            for( ;; )
            {
                Console.WriteLine( "Please select Location from list: " );

                string answer = Console.ReadLine( );
                int selection = 0;
                if( !int.TryParse( answer, out selection ) || selection > regionsCount || selection < 1 )
                {
                    Console.WriteLine( "Incorrect Location number." );
                    continue;
                }

                if( ConsoleHelper.Confirm( "Are you sure you want to select location " + regions[selection - 1] + "?" ) )
                {
                    return regions[ selection - 1 ];
                }
            }
        }

        private AzurePrepOutputs CreateEventHub( AzurePrepInputs inputs )
        {
            AzurePrepOutputs result = new AzurePrepOutputs
            {
                SBNamespace = inputs.SBNamespace
            };
            // Create Namespace
            var sbMgmt = new ServiceBusManagementClient( inputs.Credentials );

            ServiceBusNamespaceResponse nsResponse = null;

            _ConsoleBuffer.Add( string.Format( "Creating Service Bus namespace {0} in location {1}", inputs.SBNamespace, inputs.Location ) );

            try
            {
                // There is (currently) no clean error code returned when the namespace already exists
                // Check if it does
                nsResponse = sbMgmt.Namespaces.Create( inputs.SBNamespace, inputs.Location );
                _ConsoleBuffer.Add( string.Format( "Service Bus namespace {0} created.", inputs.SBNamespace ) );
            }
            catch ( Exception )
            {
                nsResponse = null;
                _ConsoleBuffer.Add( string.Format( "Service Bus namespace {0} already existed.", inputs.SBNamespace ) );
            }

            // Wait until the namespace is active
            while( nsResponse == null || nsResponse.Namespace.Status != "Active" )
            {
                nsResponse = sbMgmt.Namespaces.Get( inputs.SBNamespace );
                if( nsResponse.Namespace.Status == "Active" )
                {
                    break;
                }
                _ConsoleBuffer.Add( string.Format( "Namespace {0} in state {1}. Waiting...", inputs.SBNamespace, nsResponse.Namespace.Status ) );
                System.Threading.Thread.Sleep( 5000 );
            }

            // Get the namespace connection string 
            var nsDescription = sbMgmt.Namespaces.GetNamespaceDescription( inputs.SBNamespace );
            result.nsConnectionString = nsDescription.NamespaceDescriptions.First(
                ( d ) => String.Equals( d.AuthorizationType, "SharedAccessAuthorization" )
                ).ConnectionString;

            // Create EHs + device keys + consumer keys (WebSite*)
            var nsManager = NamespaceManager.CreateFromConnectionString( result.nsConnectionString );

            var ehDescriptionDevices = new EventHubDescription( inputs.EventHubNameDevices )
            {
                PartitionCount = 8,
            };
            ehDescriptionDevices.Authorization.Add( new SharedAccessAuthorizationRule( "D1", new List<AccessRights> { AccessRights.Send } ) );
            ehDescriptionDevices.Authorization.Add( new SharedAccessAuthorizationRule( "D2", new List<AccessRights> { AccessRights.Send } ) );
            ehDescriptionDevices.Authorization.Add( new SharedAccessAuthorizationRule( "D3", new List<AccessRights> { AccessRights.Send } ) );
            ehDescriptionDevices.Authorization.Add( new SharedAccessAuthorizationRule( "D4", new List<AccessRights> { AccessRights.Send } ) );

            ehDescriptionDevices.Authorization.Add( new SharedAccessAuthorizationRule( "WebSite", new List<AccessRights> { AccessRights.Manage, AccessRights.Listen, AccessRights.Send } ) );

            ehDescriptionDevices.Authorization.Add( new SharedAccessAuthorizationRule( "StreamingAnalytics", new List<AccessRights> { AccessRights.Manage, AccessRights.Listen, AccessRights.Send } ) );

            _ConsoleBuffer.Add( string.Format( "Creating Event Hub {0}", inputs.EventHubNameDevices ) );

            result.ehDevices = null;
            result.ehAlerts = null;

            do
            {
                try
                {
                    result.ehDevices = nsManager.CreateEventHubIfNotExists( ehDescriptionDevices );
                }
                catch ( UnauthorizedAccessException )
                {
                    _ConsoleBuffer.Add( "Service Bus connection string not valid yet. Waiting..." );
                    System.Threading.Thread.Sleep( 5000 );
                }
            } while ( result.ehDevices == null );


            var ehDescriptionAlerts = new EventHubDescription( inputs.EventHubNameAlerts )
            {
                PartitionCount = 8,
            };
            ehDescriptionAlerts.Authorization.Add( new SharedAccessAuthorizationRule( "WebSite", new List<AccessRights> { AccessRights.Manage, AccessRights.Listen, AccessRights.Send } ) );
            ehDescriptionAlerts.Authorization.Add( new SharedAccessAuthorizationRule( "StreamingAnalytics", new List<AccessRights> { AccessRights.Manage, AccessRights.Listen, AccessRights.Send } ) );

            _ConsoleBuffer.Add( string.Format( "Creating Event Hub {0}", inputs.EventHubNameAlerts ) );

            do
            {
                try
                {
                    result.ehAlerts = nsManager.CreateEventHubIfNotExists( ehDescriptionAlerts );
                }
                catch ( UnauthorizedAccessException )
                {
                    _ConsoleBuffer.Add( "Service Bus connection string not valid yet. Waiting..." );
                    System.Threading.Thread.Sleep( 5000 );
                }
            } while ( result.ehAlerts == null );

            // Create Storage Account for Event Hub Processor
            var stgMgmt = new StorageManagementClient( inputs.Credentials );
            try
            {
                _ConsoleBuffer.Add( string.Format( "Creating Storage Account {0} in location {1}",
                    inputs.StorageAccountName, inputs.Location ) );

                var resultStg = stgMgmt.StorageAccounts.Create(
                    new StorageAccountCreateParameters { Name = inputs.StorageAccountName.ToLowerInvariant(), Location = inputs.Location, AccountType = "Standard_LRS" } );

                if( resultStg.StatusCode != System.Net.HttpStatusCode.OK )
                {
                    _ConsoleBuffer.Add( string.Format( "Error creating storage account {0} in Location {1}: {2}",
                        inputs.StorageAccountName, inputs.Location, resultStg.StatusCode ) );
                    return null;
                }
            }
            catch ( CloudException ce )
            {
                if( String.Equals( ce.ErrorCode, "ConflictError", StringComparison.InvariantCultureIgnoreCase ) )
                {
                    _ConsoleBuffer.Add( string.Format( "Storage account {0} already existed.", inputs.StorageAccountName ) );
                }
                else
                {
                    throw;
                }
            }

            return result;
        }

#if AZURESTREAMANALYTICS
      
        class StreamAnalyticsEntity
        {
            public string name;
            public Dictionary<string, object> properties;
        }
        class StreamAnalyticsJob
        {
            public string location;
            public Dictionary<string, object> properties;
            public List<StreamAnalyticsEntity> inputs;
            public StreamAnalyticsEntity transformation;
            public List<StreamAnalyticsEntity> outputs;
        }
#endif

        static int Main( string[] args )
        {
            var p = new Program( );

            try
            {
                bool result = p.Run( );
                return result ? 0 : 1;
            }
            catch ( Exception e )
            {
                Console.WriteLine( "Exception {0} while creating Azure resources at {1}", e.Message, e.StackTrace );
                return 0;
            }
        }
    }
}
