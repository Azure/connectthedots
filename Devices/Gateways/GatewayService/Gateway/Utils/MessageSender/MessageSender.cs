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

//#define DEBUG_LOG



namespace Microsoft.ConnectTheDots.Gateway
{
    using System;
    using System.Text;
    using Newtonsoft.Json;
    using Common;
    using Common.Threading;
    using Microsoft.Azure.Devices.Client;

    //--//

    public class MessageSender<T> : IMessageSender<T>
    {
        private DeviceClient deviceClient;

        private static readonly string _logMesagePrefix = "MessageSender error. ";

        //--//

        private readonly string         _defaultSubject;
        private readonly string         _defaultDeviceId;
        private readonly string         _defaultDeviceDisplayName;

        public ILogger Logger
        {
            private get;
            set;
        }

        public MessageSender( string gatewayIotHubConnectionString, ILogger logger )
        {
            Logger = SafeLogger.FromLogger( logger );

#if DEBUG_LOG
            Logger.LogInfo( "Connecting to IotHub" );
#endif
            deviceClient = DeviceClient.CreateFromConnectionString(gatewayIotHubConnectionString);
            deviceClient.OpenAsync();
        }

        public TaskWrapper SendMessage( T data )
        {
            TaskWrapper result = null;

            try
            {
                if( data == null )
                {
                    return default( TaskWrapper );
                }

                string jsonData = JsonConvert.SerializeObject( data );

                result = PrepareAndSend( jsonData );
            }
            catch( Exception ex )
            {
                Logger.LogError( _logMesagePrefix + ex.Message );
            }

            return result;
        }

        public TaskWrapper SendSerialized( string jsonData )
        {
            TaskWrapper result = null;

            try
            {
                if( String.IsNullOrEmpty( jsonData ) )
                {
                    return default( TaskWrapper );
                }

                result = PrepareAndSend( jsonData );
            }
            catch( Exception ex )
            {
                Logger.LogError( _logMesagePrefix + ex.Message );
            }

            return result;
        }

        public void Close()
        {
            deviceClient = null;
        }

        private TaskWrapper PrepareAndSend( string jsonData )
        {
            var msg = PrepareMessage( jsonData );

            var sh = new SafeAction<Message>( m => deviceClient.SendEventAsync(msg), Logger );

            return TaskWrapper.Run( ( ) => sh.SafeInvoke( msg ) );
        }
        
        protected Message PrepareMessage( string serializedData, string subject = default(string), string deviceId = default(string), string deviceDisplayName = default(string) )
        {
            if( subject == default( string ) )
                subject = _defaultSubject;

            if( deviceId == default( string ) )
                deviceId = _defaultDeviceId;

            if( deviceDisplayName == default( string ) )
                deviceDisplayName = _defaultDeviceDisplayName;

            var creationTime = DateTime.UtcNow;

            Message message = null;


            if( !String.IsNullOrEmpty( serializedData ) )
            {
                message = new Message(Encoding.UTF8.GetBytes(serializedData));
                message.Properties.Add("Subject", subject);
                message.Properties.Add("CreationTime", JsonConvert.SerializeObject(creationTime));
            }

            return message;
        }
    }
}
