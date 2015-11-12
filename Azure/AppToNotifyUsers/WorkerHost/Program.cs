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
        private static EventHubReader          _EventHubReader;
        private static Timer                   _Timer;

        private static object                  _SenderLock;
        private static Web                     _SendGridTransportWeb;
        private static SmtpClient              _SmtpClient;

        private static MailAddress             _FromAddress;
        private static MailAddress[]           _ToAddress;

        private static AppConfiguration        _Config;

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

            _Config = ConfigurationLoader.GetConfig();
            if (_Config == null)
            {
                return;
            }
            _Config.ConsumerGroupPrefix += consumerGroupSuffix;

            _NotificationService =
                  (NotificationServiceType)Enum.Parse(typeof(NotificationServiceType), _Config.NotificationService);

            var credentials = new NetworkCredential(_Config.EmailServiceUserName, _Config.EmailServicePassword);

            _FromAddress = new MailAddress(_Config.MessageFromAddress, _Config.MessageFromName);

            _ToAddress = new MailAddress[_Config.SendToList.Count];
            int sendToCount = 0;
            foreach (var sendTo in _Config.SendToList)
            {
                _ToAddress[sendToCount++] = new MailAddress(sendTo);
            }

            switch (_NotificationService)
            {
                case NotificationServiceType.Smtp:
                    {
                        _SmtpClient = new SmtpClient
                        {
                            Port = _Config.SmtpEnableSSL ? 587 : 25,
                            DeliveryMethod = SmtpDeliveryMethod.Network,
                            UseDefaultCredentials = false,
                            EnableSsl = false,
                            Host = _Config.SmtpHost,
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

            _EventHubReader = new EventHubReader(_Config.ConsumerGroupPrefix, OnMessage);

            Process();
        }

        public static void Process()
        {
            _EventHubReader.Run(_Config.DeviceEHConnectionString, _Config.DeviceEHName, string.Empty);
            _EventHubReader.FailureEvent.WaitOne();
        }

        private static void OnMessage(string serializedData)
        {
            lock (_SenderLock)
            {
                try
                {
                    if (!_Config.SendToList.Any()) return;

                    string messageBody = "Message Received: \n" + serializedData;

                    if (_SmtpClient != null)
                    {
                        foreach (var mailTo in _ToAddress)
                        {
                            MailMessage myMessage = new MailMessage(_FromAddress, mailTo)
                            {
                                Subject = _Config.MessageSubject,
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
                            _Config.MessageSubject,
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
}
