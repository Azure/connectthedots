using System;
using System.Threading;
using System.Threading.Tasks;
using Gateway.DataIntake;
using Gateway.Models;
using CoreTest.Utils.Generators;
using Newtonsoft.Json;
using SharedInterfaces;

namespace DataIntakeTestMock
{
    public class DataIntakeTestMockThread : DataIntakeAbstract
    {
        private const int SLEEP_TIME_MS = 1000;
        private const int LOG_MESSAGE_RATE = 100;//should be positive

        private Func<string, int> _Enqueue;
        private bool _DoWorkSwitch;

        public DataIntakeTestMockThread( ILogger logger )
            : base( logger ) 
        {
        }

        public override bool Start( Func<string, int> enqueue )
        {
            _Enqueue = enqueue;

            _DoWorkSwitch = true;

            Task.Run( ( ) => TestRun( ) );

            return true;
        }

        public override bool Stop( )
        {
            _DoWorkSwitch = false;

            return true;
        }

        public override bool SetEndpoint( SensorEndpoint endpoint )
        {
            //we don't need any endpoints for this Data Intake
            if (endpoint == null)
                return true;

            return false;
        }

        public void TestRun()
        {
            int messagesSent = 0;
            do
            {
                SensorDataContract sensorData = RandomSensorDataGenerator.Generate();
                
                string serializedData = JsonConvert.SerializeObject(sensorData);

                _Enqueue(serializedData);

                if (++messagesSent % LOG_MESSAGE_RATE == 0)
                {
                    _Logger.LogInfo(LOG_MESSAGE_RATE + " messages sent via DataIntakeTestMock.");
                }

                Thread.Sleep(SLEEP_TIME_MS);

            } while (_DoWorkSwitch);
        }
    }
}
