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

namespace Microsoft.ConnectTheDots.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;

    public static class IPAddressHelper
    {
        public static void GetIPAddressString( ref string IPString )
        {
            const int PING_TIMEOUT = 2000;
            const int PING_RETRIES_COUNT = 100;

            IPString = string.Empty;
            string result = string.Empty;

            IEnumerable<IPAddress> localList = GetLocalIPAddressList( );
            if( localList != null && localList.Any( ) )
            {
                result += "Gateway local IP: " + localList.First( ) + '\n';
            }

            for( int step = 0; step < PING_RETRIES_COUNT; ++step )
            {
                IPAddress replyAddress;

                replyAddress = GetIPAddressByPing( "corp.microsoft.com", PING_TIMEOUT );
                if( replyAddress != null )
                {
                    result += "Gateway public IP: " + replyAddress + '\n';
                    break;
                }

                replyAddress = GetIPAddressByPing( "www.microsoft.com", PING_TIMEOUT );
                if( replyAddress != null )
                {
                    result += "Gateway public IP: " + replyAddress + '\n';
                    break;
                }
            }

            if( !string.IsNullOrEmpty( result ) )
            {
                IPString = result;
            }
        }

        public static IEnumerable<IPAddress> GetLocalIPAddressList( )
        {
            IPHostEntry ipHostEntry = Dns.GetHostEntry( string.Empty );
            if( ipHostEntry != null )
            {
                var selected = ipHostEntry.AddressList.Where( a => a.AddressFamily == AddressFamily.InterNetwork );
                return selected;
            }
            return null;
        }

        public static IPAddress GetIPAddressByPing( string hostName, int timeout )
        {
            try
            {
                Ping ping = new Ping( );
                PingReply replay = ping.Send( hostName, timeout );
                if( replay != null && replay.Status == IPStatus.Success )
                {
                    return replay.Address;
                }
            }
            catch( Exception )
            {
            }
            return null;
        }
    }
}
