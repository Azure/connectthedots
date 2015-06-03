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
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;

    //--//

    using Hyak.Common;
    using Microsoft.Azure.Management.Resources.Models;
    using Microsoft.Azure.Management.StreamAnalytics;
    using Microsoft.Azure.Management.StreamAnalytics.Models;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using Microsoft.WindowsAzure.Management.Storage;
    using Microsoft.WindowsAzure.Management.Storage.Models;
    using Microsoft.WindowsAzure.Management.ServiceBus;
    using Microsoft.WindowsAzure.Management.ServiceBus.Models;

    //--//

    using SubscriptionCloudCredentials = Microsoft.Azure.SubscriptionCloudCredentials;

    //--//

    using Microsoft.ConnectTheDots.CloudDeploy.Common;

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
                Console.WriteLine( "Enter a name for Service Bus Namespace (only letters and digits, less than 17 chars long)." );
                Console.WriteLine( "(Note that fully qualified path may also be subject to further length restrictions.)" );
                result.NamePrefix = Console.ReadLine( );
                if( string.IsNullOrEmpty( result.NamePrefix ) || !CheckNamePrefix( result.NamePrefix ) )
                {
                    Console.WriteLine( "Namespace prefix should contain only letters and digits and have length less than 17." );
                    continue;
                }
                if( ConsoleHelper.Confirm( "Are you sure you want to create a namespace called " + result.NamePrefix + "?" ) )
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
                Console.ReadLine( );
                return false;
            }

            for( ;; )
            {
                Console.WriteLine( "Do you want to create Stream Analytics jobs? (y/n)" );

                string answer = Console.ReadLine( );
                bool create;
                string request;
                if( !string.IsNullOrEmpty( answer ) && answer.ToLower( ).StartsWith( "y" ) )
                {
                    create = true;
                    request = "";
                }
                else
                {
                    create = false;
                    request = "do not ";
                }
                if( ConsoleHelper.Confirm( "Are you sure you " + request + "want to create Stream Analytics jobs?" ) )
                {
                    if( create )
                    {
                        CreateStreamAnalyticsJobs( inputs, createResults );    
                    }
                    break;
                }
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
                string suffixMessage = string.Empty;
                if( regions[currentRegion - 1] == "East US" )
                {
                    //see https://github.com/MSOpenTech/connectthedots/issues/168
                    suffixMessage = " (creating new Resource Group is not supported)";
                }
                Console.WriteLine( currentRegion + ": " + regions[ currentRegion - 1 ] + suffixMessage );
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

            int triesCount = 0;
            // Wait until the namespace is active
            while( nsResponse == null || nsResponse.Namespace.Status != "Active" )
            {
                nsResponse = sbMgmt.Namespaces.Get( inputs.SBNamespace );
                if( nsResponse.Namespace.Status == "Active" )
                {
                    break;
                }
                triesCount += 1;
                if( triesCount % 10 == 0 )
                {
                    _ConsoleBuffer.Add( "Please note that activation could last about an hour if namespace with the same name prefix was deleted recently..." );
                }
                else
                {
                    _ConsoleBuffer.Add( string.Format( "Namespace {0} in state {1}. Waiting...", inputs.SBNamespace, nsResponse.Namespace.Status ) );
                }
                
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

            _ConsoleBuffer.Add( string.Format( "Creating Event Hub {0}...", inputs.EventHubNameDevices ) );

            result.ehDevices = null;

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


            
            ConsoleHelper.AskAndPerformAction(
                "Do you want to create " + inputs.EventHubNameAlerts + " Event Hub?",
                "Are you sure you want to create " + inputs.EventHubNameAlerts + " Event Hub?",
                "Are you sure you do not want to create " + inputs.EventHubNameAlerts + " Event Hub?",
                ( ) =>
                {
                    var ehDescriptionAlerts = new EventHubDescription( inputs.EventHubNameAlerts )
                    {
                        PartitionCount = 8,
                    };
                    ehDescriptionAlerts.Authorization.Add( new SharedAccessAuthorizationRule( "WebSite", new List<AccessRights> { AccessRights.Manage, AccessRights.Listen, AccessRights.Send } ) );
                    ehDescriptionAlerts.Authorization.Add( new SharedAccessAuthorizationRule( "StreamingAnalytics", new List<AccessRights> { AccessRights.Manage, AccessRights.Listen, AccessRights.Send } ) );

                    _ConsoleBuffer.Add( string.Format( "Creating Event Hub {0}...", inputs.EventHubNameAlerts ) );
                    result.ehAlerts = null;

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
                },
                _ConsoleBuffer );
            

            // Create Storage Account for Event Hub Processor
            var stgMgmt = new StorageManagementClient( inputs.Credentials );
            try
            {
                _ConsoleBuffer.Add( string.Format( "Creating Storage Account {0} in location {1}...",
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
            catch( CloudException ce )
            {
                if( String.Equals( ce.Error.Code, "ConflictError", StringComparison.InvariantCultureIgnoreCase ) )
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

        private string SelectResourceGroup( AzurePrepInputs inputs )
        {
            Console.WriteLine( "Retrieving a list of Resource Groups..." );
            ResourceGroupExtended[] groups = AzureProvider.GetResourceGroups( inputs.Credentials );
            int count = groups.Length;

            Console.WriteLine( "Available Resource Groups: " );

            Console.WriteLine( "0: Create new Resource Group." );
            for( int current = 1; current <= count; ++current )
            {
                Console.WriteLine( current + ": " + groups[ current - 1 ].Name );
            }

            for( ;; )
            {
                Console.WriteLine( "Please select Resource Group from list: " );

                string answer = Console.ReadLine( );
                int selection = 0;
                if( !int.TryParse( answer, out selection ) || selection > count || selection < 0 )
                {
                    Console.WriteLine( "Incorrect Resource Group number." );
                    continue;
                }

                if( selection == 0 )
                {
                    if( ConsoleHelper.Confirm( "Are you sure you want to create new Resource Group?" ) )
                    {
                        string resourceGroupName;
                        for( ;; )
                        {
                            Console.WriteLine( "Enter a name for Resource Group (only letters and digits, less than 17 chars long)." );
                            Console.WriteLine( "(Note that fully qualified path may also be subject to further length restrictions.)" );
                            resourceGroupName = Console.ReadLine( );
                            if( string.IsNullOrEmpty( resourceGroupName ) || !CheckNamePrefix( resourceGroupName ) )
                            {
                                Console.WriteLine( "Namespace prefix should contain only letters and digits and have length less than 17." );
                                continue;
                            }
                            if( ConsoleHelper.Confirm( "Are you sure you want to create a Resource Group called " + resourceGroupName + "?" ) )
                            {
                                break;
                            }
                        }
                        AzureProvider.CreateResourceGroup( inputs.Credentials, resourceGroupName, inputs.Location );
                        return resourceGroupName;
                    }
                }
                else
                {
                    if( ConsoleHelper.Confirm( "Are you sure you want to select Resource Group " + groups[ selection - 1 ].Name + "?" ) )
                    {
                        return groups[ selection - 1 ].Name;
                    }    
                }
            }
        }

        private void CreateStreamAnalyticsJobs( AzurePrepInputs azurePrepIn, AzurePrepOutputs azurePrepOut )
        {
            string resourceGroupName = SelectResourceGroup( azurePrepIn );

            string path = Path.GetDirectoryName( System.Reflection.Assembly.GetEntryAssembly( ).Location );
            path += "\\..\\..\\..\\..\\StreamAnalyticsQueries";
            foreach( string filename in Directory.GetFiles( path ) )
            {
                string extension = Path.GetExtension( filename );
                if( extension != null && extension.Contains( "sql" ) )
                {
                    string nameWithoutExtension = Path.GetFileNameWithoutExtension( filename );
                    EventHubDescription ehOutput = ( filename.ToLower( ).Contains( "aggregates" ) || azurePrepOut.ehAlerts == null )
                        ? azurePrepOut.ehDevices
                        : azurePrepOut.ehAlerts;

                    if( ehOutput == null )
                    {
                        _ConsoleBuffer.Add( string.Format( " Skip creating {0} Stream Analytics job because there is no output Event Hub...", nameWithoutExtension ) );
                        continue;
                    }

                    string queryFilename = filename;
                    ConsoleHelper.AskAndPerformAction(
                        "Do you want to create " + nameWithoutExtension + " job?",
                        "Are you sure you want to create " + nameWithoutExtension + " job?",
                        "Are you sure you do not want to create " + nameWithoutExtension + " job?",
                        ( ) =>
                        {
                            string query = File.ReadAllText( queryFilename );

                            _ConsoleBuffer.Add( string.Format( "Creating {0} Stream Analytics job...", nameWithoutExtension ) );

                            CreateStreamAnalyticsJob( nameWithoutExtension, query, resourceGroupName,
                                azurePrepIn, azurePrepOut.ehDevices, ehOutput );
                        },
                        _ConsoleBuffer );
                }
            }
        }

        private void CreateStreamAnalyticsJob( string nameSuffix, string query, string resourceGroupName, AzurePrepInputs azurePrepIn, 
            EventHubDescription ehInput, EventHubDescription ehOutput )
        {
            const string inputName = "DevicesInput";
            const string outputName = "output";

            string jobName = azurePrepIn.NamePrefix + nameSuffix;
            string transformationName = jobName + "-tr";

            var computeClient = new StreamAnalyticsManagementClient( azurePrepIn.Credentials );

            var serialization = new JsonSerialization
            {
                Type = "JSON",
                Properties = new JsonSerializationProperties
                {
                    Encoding = "UTF8"
                }
            };

            List<Input> jobInputs = new List<Input>
            {
                new Input
                {
                    Name = inputName,
                    Properties = new StreamInputProperties
                    {
                        DataSource = new EventHubStreamInputDataSource
                        {
                            Properties = new EventHubStreamInputDataSourceProperties
                            {
                                EventHubName = ehInput.Path,
                                ServiceBusNamespace = azurePrepIn.SBNamespace,
                                SharedAccessPolicyName = "StreamingAnalytics",
                                SharedAccessPolicyKey = ( ehInput.Authorization.First( ( d )
                                    => String.Equals( d.KeyName, "StreamingAnalytics", StringComparison.InvariantCultureIgnoreCase) ) as SharedAccessAuthorizationRule ).PrimaryKey,
                            }
                        },
                        Serialization = serialization
                    }
                }
            };

            List<Output> jobOutputs = new List<Output>
            {
                new Output
                {
                    Name = outputName,
                    Properties = new OutputProperties
                    {
                        DataSource = new EventHubOutputDataSource
                        {
                            Properties = new EventHubOutputDataSourceProperties
                            {
                                EventHubName = ehOutput.Path,
                                ServiceBusNamespace = azurePrepIn.SBNamespace,
                                SharedAccessPolicyName = "StreamingAnalytics",
                                SharedAccessPolicyKey = ( ehOutput.Authorization.First( ( d )
                                    => String.Equals( d.KeyName, "StreamingAnalytics", StringComparison.InvariantCultureIgnoreCase) ) as SharedAccessAuthorizationRule ).PrimaryKey,
                            }
                        },
                        Serialization = serialization
                    }
                }
            };

            bool created = true;
            try
            {
                var jobCreateResponse = computeClient.StreamingJobs.CreateOrUpdateAsync(
                    resourceGroupName,
                    new JobCreateOrUpdateParameters
                    {
                        Job = new Job
                        {
                            Name = jobName,
                            Location = azurePrepIn.Location,
                            Properties = new JobProperties
                            {
                                Sku = new Sku
                                {
                                    //should be "standart" according to https://msdn.microsoft.com/en-us/library/azure/dn834994.aspx
                                    Name = "standard"
                                },
                                EventsOutOfOrderPolicy = "drop",
                                EventsOutOfOrderMaxDelayInSeconds = 10,
                                Inputs = jobInputs,
                                Outputs = jobOutputs,
                                Transformation = new Transformation
                                {
                                    Name = transformationName,
                                    Properties = new TransformationProperties
                                    {
                                        Query = query,
                                        StreamingUnits = 1
                                    }
                                }
                            }
                        }

                    }
                    ).Result;
            }
            catch( Exception ex )
            {
                _ConsoleBuffer.Add( "Exception on creation Stream Analytics Job " + jobName + ": " + ex.Message );
                _ConsoleBuffer.Add( "Inner exception message: " + ex.InnerException.Message );
                created = false;
            }
            if( created )
            {
                _ConsoleBuffer.Add( "Stream Analytics job " + jobName + " created." );
            }
        }

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
                Console.WriteLine( "Please hit enter to close." );
                Console.ReadLine( );
                return 0;
            }
        }
    }
}
