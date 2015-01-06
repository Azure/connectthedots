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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web;

using Microsoft.Web.WebSockets;

using Newtonsoft.Json;

namespace WebClient
{
    public class WebSocketConnectController : ApiController
    {
        // GET api/<controller>
        public HttpResponseMessage Get(string clientId)
        {
            HttpContext.Current.AcceptWebSocketRequest(new MyWebSocketHandler());
            return Request.CreateResponse(HttpStatusCode.SwitchingProtocols);
        }
    }

    class MyWebSocketHandler : WebSocketHandler
    {
        private static WebSocketCollection _clients = new WebSocketCollection();

        public string DeviceFilter = null;

        public MyWebSocketHandler()
        {
        }

        public override void OnOpen()
        {
            _clients.Add(this);
            ResendDataToClient();
        }

        public void ResendDataToClient()
        {
            var bufferedMessages = WebSocketEventProcessor.GetAllBufferedMessages();

            this.Send(JsonConvert.SerializeObject(new Dictionary<string, object> 
                { 
                    { "bulkData", true }
                }
            ));

            foreach (var message in bufferedMessages)
            {
                this.SendFiltered(message);
            }

            this.Send(JsonConvert.SerializeObject(new Dictionary<string, object> 
                { 
                    { "bulkData", false}
                }
            ));
        }

        public override void OnClose()
        {
            _clients.Remove(this);
            base.OnClose();
        }

        public override void OnMessage(string message)
        {
            try
            {
                var messageDictionary = (IDictionary<string, object>)
                    JsonConvert.DeserializeObject(message, typeof(IDictionary<string, object>));

                if (messageDictionary.ContainsKey("MessageType"))
                {
                    switch (messageDictionary["MessageType"] as string)
                    {
                        case "LiveDataSelection":
                            DeviceFilter = messageDictionary["DeviceName"] as string;
                            break;
                        default:
                            Trace.TraceError("Client message with unknown message type: {0} - {1}", messageDictionary["MessageType"], message);
                            break;
                    }
                }
                else
                {
                    Trace.TraceError("Client message without message type: {0}", message);
                }
            }
            catch (Exception e)
            {
                Trace.TraceError("Error processing client message: {0} - {1}", e.Message, message);
            }
            ResendDataToClient();
        }

        public void SendFiltered(IDictionary<string, object> message)
        {
            if (   !message.ContainsKey("dspl")
                || (this.DeviceFilter != null 
                    && (
                        String.Equals(this.DeviceFilter, "all", StringComparison.InvariantCultureIgnoreCase))
                        || String.Equals(this.DeviceFilter, message["dspl"])))
            {
                this.Send(JsonConvert.SerializeObject(message));
            }
        }

        public static void SendToClients(IDictionary<string, object> message)
        {
            foreach (MyWebSocketHandler client in _clients)
            {
                client.SendFiltered(message);
            }
        }
    }

}