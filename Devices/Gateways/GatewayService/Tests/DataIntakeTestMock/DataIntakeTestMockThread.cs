using System;
using System.Threading;
using System.Threading.Tasks;
using Gateway.DataIntake;
using Gateway.Models;
using Gateway.Utils.Generators;
using Gateway.Utils.Logger;
using Newtonsoft.Json;

namespace DataIntakeTestMock
{
    public class DataIntakeTestMockThread : IDataIntake
    {
        private const int SLEEP_TIME_MS = 1000;
        private const int LOG_MESSAGE_RATE = 100;//should be positive

        private static ILogger _Logger;
        private static Func<string, int> _Enqueue;
        private static Func<bool> _DoWorkSwitch;

        public bool Start(Func<string, int> enqueue, ILogger logger, Func<bool> doWorkSwitch)
        {
            _Enqueue = enqueue;
            _Logger = logger;
            _DoWorkSwitch = doWorkSwitch;

            Task.Run(() => TestRun());
            return true;
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
                    if (LOG_MESSAGE_RATE == 1)
                        _Logger.LogInfo("Message sent via DataIntakeTestMock: " + serializedData);
                    else
                        _Logger.LogInfo(LOG_MESSAGE_RATE + " messages sent via DataIntakeTestMock.");
                }

                Thread.Sleep(SLEEP_TIME_MS);
            } while (_DoWorkSwitch());
        }
    }

}
