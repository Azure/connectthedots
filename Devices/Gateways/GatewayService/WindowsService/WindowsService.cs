namespace Microsoft.ConnectTheDots.GatewayService
{
    using System;
    using System.ServiceModel.Web;
    using System.ServiceProcess;
    using System.Threading.Tasks;
    using System.Configuration;
    using Microsoft.ConnectTheDots.Gateway;
    using Microsoft.ConnectTheDots.Common;

    //--//

    public class WindowsService : ServiceBase
    {
        private const int STOP_TIMEOUT_MS = 5000; // ms

        private static WebServiceHost _WebHost;

        private readonly ILogger _Logger;

        private readonly GatewayQueue<QueuedItem> _GatewayQueue;

        private readonly AMQPSender<SensorDataContract> _AMPQSender;

        private readonly EventProcessor _BatchSenderThread;

        private readonly DataIntakeLoader _DataIntakeLoader;

        public WindowsService( ILogger logger )
        {
            if(logger == null)
            {
                throw new ArgumentException( "Cannot run service without logging" );
            }

            _Logger = logger;

            if(logger is TunableLogger)
            {
                TunableLogger.LoggingLevel loggingLevel = TunableLogger.LevelFromString( ConfigurationManager.AppSettings.Get( "LoggingLevel" ) );

                ( ( TunableLogger )logger ).Level = (loggingLevel != TunableLogger.LoggingLevel.Undefined) ? loggingLevel : TunableLogger.LoggingLevel.Errors;
            }

            try
            {
                TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

                // Name the Windows Service
                ServiceName = Constants.WindowsServiceName;

                _GatewayQueue = new GatewayQueue<QueuedItem>();
                AMQPConfig amqpConfig = Loader.GetAMQPConfig();

                if (amqpConfig == null)
                {
                    _Logger.LogError("AMQP configuration is missing");
                    return;
                }
                _AMPQSender = new AMQPSender<SensorDataContract>(
                                                    amqpConfig.AMQPSAddress,
                                                    amqpConfig.EventHubName,
                                                    amqpConfig.EventHubMessageSubject,
                                                    amqpConfig.EventHubDeviceId,
                                                    amqpConfig.EventHubDeviceDisplayName,
                                                    _Logger
                                                    );
                _BatchSenderThread = new BatchSenderThread<QueuedItem, SensorDataContract>(
                                                    _GatewayQueue,
                                                    _AMPQSender,
                                                    null,//m => DataTransforms.AddTimeCreated(DataTransforms.SensorDataContractFromQueuedItem(m, _Logger)),
                                                    new Func<QueuedItem, string>( m => m.JsonData ),
                                                    _Logger);

                _DataIntakeLoader = new DataIntakeLoader(Loader.GetSources(), Loader.GetEndpoints(), _Logger); 
            }
            catch (Exception ex)
            {
                _Logger.LogInfo("Exception creating WindowsService: " + ex.Message);
            }
        }

        protected override void OnStart(string[] args)
        {
            _Logger.LogInfo("Service starting... ");

            if (_WebHost != null)
            {
                _WebHost.Close();
            }

            _AMPQSender.LogMessagePrefix = Constants.LogMessageTexts.AMQPSenderErrorPrefix;

            _BatchSenderThread.Start();

            _WebHost = new WebServiceHost( typeof( Microsoft.ConnectTheDots.Gateway.GatewayService ) );
            Gateway.GatewayService service = new Microsoft.ConnectTheDots.Gateway.GatewayService(
                _GatewayQueue,
                _BatchSenderThread,
                m => DataTransforms.QueuedItemFromSensorDataContract(
                        DataTransforms.AddTimeCreated(DataTransforms.SensorDataContractFromString(m, _Logger)), _Logger)
            );
            _WebHost.Description.Behaviors.Add(new ServiceBehavior(() => service));

            service.Logger = _Logger;
            service.OnDataInQueue += OnData;

            _WebHost.Open();

            _DataIntakeLoader.StartAll( service.Enqueue ); 
	    
            _Logger.LogInfo("...started");
        }

        protected override void OnStop()
        {
            _Logger.LogInfo("Service stopping... ");

            _DataIntakeLoader.StopAll( ); 

            // close web host first (message intake)
            if (_WebHost != null)
            {
                _WebHost.Close();
                _WebHost = null;
            }

            // shutdown processor (message processing)
            _BatchSenderThread.Logger = _Logger;
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
            ILogger logger = null;

            try
            {
                logger = TunableLogger.FromLogger(
                    SafeLogger.FromLogger( EventLogger.Instance )
                    );

                logger.LogInfo( "Creating WindowsService..." );

                Run( new WindowsService( logger ) ); 
            }
            catch(Exception ex)
            {
                if (logger != null)
                {
                    logger.LogError( ex.ToString( ) ); 
                }

                // just return...
            }
        }

        private void OnUnobservedTaskException( object sender, UnobservedTaskExceptionEventArgs e )
        {
            // prevent exception escalation
            e.SetObserved( );

            _Logger.LogError( String.Format( "Task Exception: '{0}'\r\nTrace:\r\n{1}", e.Exception.Message, e.Exception.StackTrace ) );
        }
    }
}
