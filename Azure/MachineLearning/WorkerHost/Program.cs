using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;

namespace WorkerHost
{
    public class WorkerHost
    {
        public class Configuration
        {
            public string AlertEHConnectionString;
            public string AlertEHName;
            public string DeviceEHConnectionString;
            public string DeviceEHName;
            public string AnomalyDetectionApiUrl;
            public string AnomalyDetectionAuthKey;
            public string LiveId;
            public bool UseMarketApi;
        }

        private static Analyzer        _analyzer;
        private static EventHubReader  _eventHubReader;
        private static Timer           _timer;

        static void Main()
        {
            var config = new Configuration();
            
            config.AlertEHConnectionString = ConfigurationManager.AppSettings.Get("Microsoft.ServiceBus.ConnectionStringAlerts");
            config.AlertEHName = ConfigurationManager.AppSettings.Get("Microsoft.ServiceBus.EventHubAlerts");

            config.DeviceEHConnectionString = ConfigurationManager.AppSettings.Get("Microsoft.ServiceBus.ConnectionStringDevices");
            config.DeviceEHName = ConfigurationManager.AppSettings.Get("Microsoft.ServiceBus.EventHubDevices"); ;

            config.AnomalyDetectionApiUrl = ConfigurationManager.AppSettings.Get("AnomalyDetectionApiUrl");
            config.AnomalyDetectionAuthKey = ConfigurationManager.AppSettings.Get("AnomalyDetectionAuthKey");
            config.LiveId = ConfigurationManager.AppSettings.Get("LiveId");

            bool.TryParse(ConfigurationManager.AppSettings.Get("UseMarketApi"), out config.UseMarketApi);

            _analyzer = new Analyzer(config.AnomalyDetectionApiUrl, config.AnomalyDetectionAuthKey,
                config.LiveId, config.UseMarketApi);
            _eventHubReader = new EventHubReader();

            Process(config);
        }

        public static void Process(Configuration config)
        {
            var alertEventHub = EventHubClient.CreateFromConnectionString(config.AlertEHConnectionString, config.AlertEHName);

            Trace.TraceInformation("Starting to receive messages...");
            _eventHubReader.Run(config.DeviceEHConnectionString, config.DeviceEHName);

            var timerInterval = TimeSpan.FromSeconds(1);
            var alertLastTimes = new Dictionary<string, DateTime>();

            TimerCallback timerCallback = state =>
            {

                var historicData = _eventHubReader.GetHistoricData();

                try
                {
                    var tasks = historicData.ToDictionary(kvp => kvp.Key, kvp => _analyzer.Analyze(kvp.Value));

                    Task.WaitAll(tasks.Values.ToArray());

                    foreach (var kvp in tasks)
                    {
                        var key = kvp.Key;
                        var alerts = kvp.Value.Result;

                        DateTime alertLastTime;
                        if (!alertLastTimes.TryGetValue(@key, out alertLastTime))
                        {
                            alertLastTime = DateTime.MinValue;
                        }

                        foreach (var alert in alerts)
                        {
                            if ((alert.Time - alertLastTime).TotalSeconds > 30)
                            {
                                Trace.TraceInformation("Alert - {0}", alert.ToString());

                                alertEventHub.Send(
                                    new EventData(Encoding.UTF8.GetBytes(
                                        OutputResults(key, historicData[key].LastOrDefault(),alert))));

                                alertLastTime = alert.Time;
                                alertLastTimes[@key] = alertLastTime;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
#if DEBUG_LOG
                    Trace.TraceError(e.Message);
                    Trace.TraceError(e.ToString());
#endif
                    //throw;
                }

                _timer.Change((int)timerInterval.TotalMilliseconds, Timeout.Infinite);
            };

            _timer = new Timer(timerCallback, null, Timeout.Infinite, Timeout.Infinite);
            _timer.Change(0, Timeout.Infinite);

            Trace.TraceInformation("Reading events from Event Hub (press ctrl+c to abort)");

            var exitEvent = new ManualResetEvent(false);
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                exitEvent.Set();
            };

            int index = WaitHandle.WaitAny(new[] {exitEvent, _eventHubReader.FailureEvent});

            Trace.TraceInformation("Exiting...");
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            Thread.Sleep(timerInterval);
            _timer.Dispose();
            _eventHubReader.Close();
        }

        public static IEnumerable<IEnumerable<T>> Batch<T>(IEnumerable<T> collection, int batchSize)
        {
            List<T> nextbatch = new List<T>(batchSize);
            foreach (T item in collection)
            {
                nextbatch.Add(item);
                if (nextbatch.Count == batchSize)
                {
                    yield return nextbatch;
                    nextbatch = new List<T>();
                }
            }
            if (nextbatch.Count > 0)
                yield return nextbatch;
        }

        private static string OutputResults(string from, SensorDataContract sensorMeta, AnomalyRecord alert)
        {
            return JsonConvert.SerializeObject(
                new
                {
                    guid = @from,
                    displayname = sensorMeta.DisplayName,
                    measurename = sensorMeta.MeasureName,
                    unitofmeasure = sensorMeta.UnitOfMeasure,
                    location = sensorMeta.Location,
                    organization = sensorMeta.Organization,

                    timecreated = alert.Time.ToLocalTime(),
                    value = alert.Data,
                    alerttype = "MLModelAlert",
                    message = "Anomaly detected by Machine Learning model."
                });
        }

    }
}
