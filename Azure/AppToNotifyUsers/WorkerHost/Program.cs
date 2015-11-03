//#define DEBUG_LOG
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;
using Microsoft.WindowsAzure.ServiceRuntime;
using SendGrid;

namespace WorkerHost
{
    public class WorkerHost : RoleEntryPoint
    {
        public class Configuration
        {
            public string DeviceEHConnectionString;
            public string DeviceEHName;

            public string SendGridUserName;
            public string SendGridPassword;

            public string MessageFromAddress;
            public string MessageFromName;
            public string MessageSubject;
            public string ConsumerGroupPrefix;
            
            public IList<string> MailToList;
        }

        private static EventHubReader _eventHubReader;
        private static Timer _timer;
        private static Web _SendGridTransportWeb;
        private static Configuration config;

        static void Main()
        {
            StartHost("L0cal");
        }

        public override void Run()
        {
            StartHost("R0le");
        }

        private static void StartHost(string consumerGroupSuffix)
        {
            Trace.WriteLine("Starting Worker...");
#if DEBUG_LOG
            RoleEnvironment.TraceSource.TraceInformation("Starting Worker...");
#endif
            config = new Configuration
            {
                DeviceEHConnectionString =
                    ConfigurationManager.AppSettings.Get("Microsoft.ServiceBus.ConnectionStringDevices"),
                DeviceEHName = ConfigurationManager.AppSettings.Get("Microsoft.ServiceBus.EventHubDevices"),
                SendGridUserName = ConfigurationManager.AppSettings.Get("SendGridUserName"),
                SendGridPassword = ConfigurationManager.AppSettings.Get("SendGridPassword"),
                MessageFromAddress = ConfigurationManager.AppSettings.Get("MessageFromAddress"),
                MessageFromName = ConfigurationManager.AppSettings.Get("MessageFromName"),
                MessageSubject = ConfigurationManager.AppSettings.Get("MessageSubject"),
                MailToList = ConfigurationLoader.GetMailToList(),
                ConsumerGroupPrefix = ConfigurationManager.AppSettings.Get("ConsumerGroupPrefix") + consumerGroupSuffix,
            };

            var credentials = new NetworkCredential(config.SendGridUserName, config.SendGridPassword);
            _SendGridTransportWeb = new Web(credentials);

            _eventHubReader = new EventHubReader(config.ConsumerGroupPrefix, OnMessage);

             Process();
        }

        public static void Process()
        {
            _eventHubReader.Run(config.DeviceEHConnectionString, config.DeviceEHName, string.Empty);
            _eventHubReader.FailureEvent.WaitOne();
        }

        private static void OnMessage(string serializedData)
        {
            try
            {
                if (config.MailToList.Any())
                {
                    SendGridMessage myMessage = new SendGridMessage();
                    myMessage.AddTo(config.MailToList);

                    myMessage.From = new MailAddress(config.MessageFromAddress, config.MessageFromName);
                    myMessage.Subject = config.MessageSubject;
                    myMessage.Text = "Message Received: \n" + serializedData;

                    _SendGridTransportWeb.DeliverAsync(myMessage).Wait();
                }
            }
            catch (Exception)
            {
                Trace.WriteLine("Exception on mail sending...");
            }
        }
    }
}
