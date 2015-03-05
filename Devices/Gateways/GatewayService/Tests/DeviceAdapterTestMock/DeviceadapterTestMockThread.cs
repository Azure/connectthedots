namespace Microsoft.ConnectTheDots.Test
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Microsoft.ConnectTheDots.Common;
    using Microsoft.ConnectTheDots.Gateway;

    //--//

    public class DeviceAdapterTestMock : DeviceAdapterAbstract
    {
        private const int SLEEP_TIME_MS = 1000;
        private const int LOG_MESSAGE_RATE = 100;//should be positive

        //--//

        private Func<string, int> _Enqueue;
        private bool              _DoWorkSwitch;

        //--//

        public DeviceAdapterTestMock( ILogger logger )
            : base( logger )
        {
        }

        public override bool Start( Func<string, int> enqueue )
        {
            _Enqueue = enqueue;

            _DoWorkSwitch = true;

            var sh = new SafeAction<int>( ( t ) => TestRun( t ), _logger );

            Task.Run( ( ) => sh.SafeInvoke( SLEEP_TIME_MS ) );

            return true;
        }

        public override bool Stop( )
        {
            _DoWorkSwitch = false;

            return true;
        }

        public override bool SetEndpoint( SensorEndpoint endpoint = null )
        {
            //we don't need any endpoints for this Data Intake
            if( endpoint == null )
                return true;

            return false;
        }

        public void TestRun( int sleepTime )
        {
            int messagesSent = 0;
            do
            {
                SensorDataContract sensorData = RandomSensorDataGenerator.Generate( );

                string serializedData = JsonConvert.SerializeObject( sensorData );

                _Enqueue( serializedData );

                if( ++messagesSent % LOG_MESSAGE_RATE == 0 )
                {
                    _logger.LogInfo( LOG_MESSAGE_RATE + " messages sent via DeviceAdapterTestMock." );
                }

                Thread.Sleep( sleepTime );

            } while( _DoWorkSwitch );
        }
    }
}
