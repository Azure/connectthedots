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
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    //--//

    using Microsoft.Azure;
    using Microsoft.Azure.Management.Resources;
    using Microsoft.Azure.Management.Resources.Models;
    using Microsoft.WindowsAzure.Management.ServiceBus;
    using Microsoft.WindowsAzure.Management.ServiceBus.Models;

    //--//

    public static class AzureProvider
    {
        public static string[] GetRegions( SubscriptionCloudCredentials creds )
        {
            var sbMgmt = new ServiceBusManagementClient( creds );
            var regionsResponse = sbMgmt.GetServiceBusRegionsAsync( ).Result;

            int currentRegion = 0, regionsCount = regionsResponse.Count( );
            string[] regions = new string[ regionsCount ];
            foreach( var region in regionsResponse )
            {
                regions[ currentRegion++ ] = region.FullName;
            }

            return regions;
        }

        public static ResourceGroupExtended CreateResourceGroup( SubscriptionCloudCredentials creds, string name, string location )
        {
            var resourceMgmtClient = new ResourceManagementClient( creds );
            var resourceGroupsResponse = resourceMgmtClient.ResourceGroups.CreateOrUpdate( name, new ResourceGroup { Location = location } );
            
            return resourceGroupsResponse.ResourceGroup;
        }

        public static ResourceGroupExtended[] GetResourceGroups( SubscriptionCloudCredentials creds )
        {
            var resourceMgmtClient = new ResourceManagementClient( creds );
            var resourceGroupsResponse = resourceMgmtClient.ResourceGroups.ListAsync( new ResourceGroupListParameters( ) ).Result;

            int currentGroup = 0, resourceGroupsCount = resourceGroupsResponse.ResourceGroups.Count;
            ResourceGroupExtended[] resourceGroups = new ResourceGroupExtended[ resourceGroupsCount ];
            foreach( var group in resourceGroupsResponse.ResourceGroups )
            {
                resourceGroups[ currentGroup++ ] = group;
            }

            return resourceGroups;
        }

        public static ServiceBusNamespace[] GetNamespaces( SubscriptionCloudCredentials creds )
        {
            var sbMgmt = new ServiceBusManagementClient( creds );
            var regionsResponse = sbMgmt.Namespaces.List( );

            int currentNamespace = 0, namespaceCount = regionsResponse.Count( );
            ServiceBusNamespace[] namespaces = new ServiceBusNamespace[ namespaceCount ];
            foreach( var region in regionsResponse )
            {
                namespaces[ currentNamespace++ ] = region;
            }

            return namespaces;
        }
    }
}
