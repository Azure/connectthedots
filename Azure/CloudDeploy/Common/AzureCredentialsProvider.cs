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

namespace Microsoft.ConnectTheDots.CloudDeploy.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.Subscriptions;
    using Microsoft.WindowsAzure.Subscriptions.Models;

    //--//

    public static class AzureCredentialsProvider
    {
        public static SubscriptionCloudCredentials GetUserSubscriptionCredentials( )
        {
            TokenCloudCredentials toFoundSubscriptions = AzureCredentialsProvider.GetCredentialsByUserADAuth( );

            IList<SubscriptionListOperationResponse.Subscription> subscriptions =
                AzureCredentialsProvider.GetSubscriptionList( toFoundSubscriptions ).Result;

            if( !subscriptions.Any( ) )
            {
                Console.WriteLine( "No available subscriptions." );
                return null;
            }
            Console.WriteLine( "Please select one of available subscriptions: " );
            int listSize = 1;
            foreach( var subscription in subscriptions )
            {
                Console.WriteLine( listSize + ": " + subscription.SubscriptionName );
                listSize++;
            }

            string answer = Console.ReadLine( );
            int selection = 0;
            if( !int.TryParse( answer, out selection ) || selection >= listSize )
            {
                return null;
            }

            TokenCloudCredentials result =
                AzureCredentialsProvider.GetCredentialsByUserADAuth( subscriptions[ selection - 1 ].SubscriptionId );

            return result;
        }

        public async static Task<IList<SubscriptionListOperationResponse.Subscription>>
            GetSubscriptionList( SubscriptionCloudCredentials credentials )
        {
            IList<SubscriptionListOperationResponse.Subscription> ret = null;

            using( var subscriptionClient = new SubscriptionClient( credentials ) )
            {
                var listSubscriptionResults =
                    await subscriptionClient.Subscriptions.ListAsync( );

                var subscriptions = listSubscriptionResults.Subscriptions;

                ret = subscriptions;
            }

            return ret;
        }

        public static SubscriptionCloudCredentials GetCredentialsByPublishSettingFile( string fileName )
        {
            var doc = new XmlDocument( );
            doc.Load( fileName );
            var certNode = doc.SelectSingleNode("/PublishData/PublishProfile/@ManagementCertificate" );
            // Some publishsettings files (with multiple subscriptions?) have the management publisherCertificate under the Subscription
            if( certNode == null )
            {
                certNode =
                doc.SelectSingleNode(
                    "/PublishData/PublishProfile/Subscription/@ManagementCertificate" );
            }

            X509Certificate2 ManagementCertificate = new X509Certificate2( Convert.FromBase64String( certNode.Value ) );
            var subNode =
                doc.SelectSingleNode( "/PublishData/PublishProfile/Subscription/@Id" );
            string SubscriptionId = subNode.Value;

            // Obtain management via .publishsettings file from https://manage.windowsazure.com/publishsettings/index?schemaversion=2.0
            CertificateCloudCredentials creds = new CertificateCloudCredentials( SubscriptionId, ManagementCertificate );
            return creds;
        }

        public static TokenCloudCredentials GetCredentialsByUserADAuth( string subscriptionId = null )
        {
            //ClientId and RedirectURI of this app
            const string AUTH_CLIENT_ID = "08d635ff-dbfc-40af-874b-9c04831b2b38";
            const string AUTH_REDIRECT_URI = "htttp://localhost/ctd";
            //TenantId
            const string AUTH_TENANT = "72f988bf-86f1-41af-91ab-2d7cd011db47";

            string token = GetAuthHeader( AUTH_CLIENT_ID, AUTH_REDIRECT_URI, AUTH_TENANT );

            var cred = ( subscriptionId == null )
                ? new TokenCloudCredentials( token ) : new TokenCloudCredentials( subscriptionId, token );
            return cred;
        }

        private static string GetAuthHeader( string clientId, string redirectUri, string tenant )
        {
            AuthenticationResult result = null;
            AuthenticationContext context = new AuthenticationContext(
                string.Format( "https://login.windows.net/{0}", tenant )
                );
            
            result = context.AcquireTokenAsync(
                resource: "https://management.core.windows.net/",
                clientId: clientId,
                redirectUri: new Uri( redirectUri ),
                parameters: new AuthorizationParameters( PromptBehavior.Auto, null )
            ).Result;

            return result.CreateAuthorizationHeader( ).Substring( "Bearer ".Length );
        }
    }
}
