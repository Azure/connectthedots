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


            public string NotificationService;
            public string EmailServiceUserName;
            public string EmailServicePassword;

            public string SmtpHost;
            public string MessageFromAddress;
            public string MessageFromName;
            public string MessageSubject;
            public string ConsumerGroupPrefix;
            
            public IList<string> MailToList;
        }

        private static EventHubReader _eventHubReader;
        private static Timer _timer;
        
        private static Web _SendGridTransportWeb;
        private static SmtpClient _SmtpClient;

        private static MailAddress _FromAddress;
        private static MailAddress[] _ToAddress;

        private static Configuration config;

        private static NotificationServiceType _NotificationService;
        enum NotificationServiceType
        {
            Smtp = 1,
            SendGridWeb = 2
        }

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
                    ConfigurationManager.AppSettings.Get("Microsoft.ServiceBus.EventHubConnectionString"),
                DeviceEHName = ConfigurationManager.AppSettings.Get("Microsoft.ServiceBus.EventHubToMonitor"),
                NotificationService = ConfigurationManager.AppSettings.Get("NotificationService"),
                EmailServiceUserName = ConfigurationManager.AppSettings.Get("EmailServiceUserName"),
                EmailServicePassword = ConfigurationManager.AppSettings.Get("EmailServicePassword"),
                SmtpHost = ConfigurationManager.AppSettings.Get("SmtpHost"),
                MessageFromAddress = ConfigurationManager.AppSettings.Get("MessageFromAddress"),
                MessageFromName = ConfigurationManager.AppSettings.Get("MessageFromName"),
                MessageSubject = ConfigurationManager.AppSettings.Get("MessageSubject"),
                MailToList = ConfigurationLoader.GetMailToList(),
                ConsumerGroupPrefix = ConfigurationManager.AppSettings.Get("ConsumerGroupPrefix") + consumerGroupSuffix,
            };

            _NotificationService =
                  (NotificationServiceType)Enum.Parse(typeof(NotificationServiceType), config.NotificationService);

            var credentials = new NetworkCredential(config.EmailServiceUserName, config.EmailServicePassword);

            _FromAddress = new MailAddress(config.MessageFromAddress, config.MessageFromName);

            _ToAddress = new MailAddress[config.MailToList.Count];
            int mailToCount = 0;
            foreach (var mailTo in config.MailToList)
            {
                _ToAddress[mailToCount++] = new MailAddress(mailTo);
            }

            switch (_NotificationService)
            {
                case NotificationServiceType.Smtp:
                    {
                        _SmtpClient = new SmtpClient
                        {
                            Port = 587,
                            DeliveryMethod = SmtpDeliveryMethod.Network,
                            UseDefaultCredentials = false,
                            EnableSsl = true,
                            Host = config.SmtpHost,
                            Credentials = credentials
                        };
                    }
                    break;
                case NotificationServiceType.SendGridWeb:
                    {
                        _SendGridTransportWeb = new Web(credentials);
                    }
                    break;
            }

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
                if (!config.MailToList.Any()) return;

                string messageBody = "Message Received: \n" + serializedData;

                if (_SmtpClient != null)
                {
                    foreach (var mailTo in _ToAddress)
                    {
                        MailMessage myMessage = new MailMessage(_FromAddress, mailTo)
                        {
                            Subject = config.MessageSubject,
                            Body = messageBody
                        };

                        _SmtpClient.Send(myMessage);
                    }
                }

                if (_SendGridTransportWeb != null)
                {
                    SendGridMessage myMessage = new SendGridMessage(
                        _FromAddress,
                        _ToAddress,
                        config.MessageSubject,
                        string.Empty,
                        messageBody
                        );

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
