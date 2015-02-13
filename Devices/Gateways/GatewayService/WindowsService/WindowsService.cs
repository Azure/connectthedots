using System;
using System.ServiceModel.Web;
using System.ServiceProcess;
using WindowsService.Utils;
using WindowsService.Utils.Logger;
using Gateway;
using Gateway.Models;
using Gateway.ServiceInstantiation;
using Gateway.Utils.Logger;
using Gateway.Utils.MessageSender;
using Gateway.Utils.Queue;

namespace WindowsService
{
    public class WindowsService : ServiceBase
    {
        private static WebServiceHost _WebHost;
        private static readonly ILogger _Logger = EventLogger.Instance;

        private static readonly GatewayQueue<QueuedItem> _GatewayQueue
            = new GatewayQueue<QueuedItem>();

        private static readonly AMQPSender<SensorDataContract> _AMPQSender
            = new AMQPSender<SensorDataContract>(Constants.AMQPSAddress,
                Constants.EventHubName, Constants.EventHubMessageSubject,
                Constants.EventHubDeviceId, Constants.EventHubDeviceDisplayName);

        private static readonly EventProcessor _BatchSenderThread
            = new BatchSenderThread<QueuedItem, SensorDataContract>(_GatewayQueue, _AMPQSender, null,
                new Func<QueuedItem, string>( m => m.JsonData));

        private static readonly DataIntakeLoader _DataIntakeLoader
            = new DataIntakeLoader(_Logger);

        private const int STOP_TIMEOUT_MS = 5000; // ms

        public WindowsService()
        {
            // Name the Windows Service
            ServiceName = Constants.WindowsServiceName;
        }

        protected override void OnStart(string[] args)
        {
            _Logger.LogInfo("Service starting... ");

            if (_WebHost != null)
            {
                _WebHost.Close();
            }

            _AMPQSender.Logger = _Logger;
            _AMPQSender.LogMessagePrefix = Constants.LogMessageTexts.AMQPSenderErrorPrefix;

            _BatchSenderThread.Logger = _Logger;
            _BatchSenderThread.Start();

            _WebHost = new WebServiceHost(typeof(Gateway.GatewayService));
            Gateway.GatewayService service = new Gateway.GatewayService(_GatewayQueue, _BatchSenderThread);
            _WebHost.Description.Behaviors.Add(new ServiceBehavior(() => service));

            service.Logger = _Logger;
            service.OnDataInQueue += OnData;


            _WebHost.Open();

            _DataIntakeLoader.Start(service.Enqueue, _Logger, DoWork);
	    
	    _Logger.LogInfo("...started");
        }

        private static bool _DoWork = true;
        public static bool DoWork()
        {
            //TODO: test stop indicator
            return _DoWork;
        }

        protected override void OnStop()
        {
            _Logger.LogInfo( "Service stopping... " );

            _DoWork = false;

            // close web host first (message intake)
            if (_WebHost != null)
            {
                _WebHost.Close();
                _WebHost = null;
            }

            // shutdown processor (message processing)
            _BatchSenderThread.Stop(STOP_TIMEOUT_MS);

            // shut down connection to event hub last
            if (_AMPQSender != null)
            {
                _AMPQSender.Close();
            }

            _Logger.LogInfo("...stopped");
        }

        protected virtual void OnData(QueuedItem data)
        {
            // LORENZO: test behaviours such as accumulating data an processing in batch
            _BatchSenderThread.Process();
        }

        static void Main(string[] args)
        {
            Run(new WindowsService());
        }
    }
}
