//#define DEBUG_LOG

using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Microsoft.WindowsAzure.Diagnostics;

namespace WorkerHost
{
    class ADResult
    {
        [DataMember]
        public string table { get; set; }

        public List<AnomalyRecord> GetADResults()
        {
            var rowDelim = ";";
            var colDelim = ",";
            var rows = table.Split(new string[] { rowDelim }, StringSplitOptions.RemoveEmptyEntries);

            List<AnomalyRecord> series = new List<AnomalyRecord>();
            for (int i = 0; i < rows.Length; i++)
            {
                var row = rows[i].Replace("\"", "").Trim();
                if (i == 0 || row.Length == 0)
                {
                    continue; // ignore headers and empty rows
                }

                var cols = row.Split(new string[] { colDelim }, StringSplitOptions.RemoveEmptyEntries);
                series.Add(AnomalyRecord.Parse(cols));
            }
            return series;
        }
    }

    class Analyzer
    {
        private static string _detectorUrl;
        private static string _detectorAuthKey;
        private static string _liveId;
        private static string _spikeDetectorParams;
        private static bool _useMarketApi;

        public Analyzer(string anomalyDetectionApiUrl, string anomalyDetectionAuthKey, string liveId, bool useMarketApi, string tukeyThresh, string zscoreThresh)
        {
            _detectorUrl = anomalyDetectionApiUrl;
            _detectorAuthKey = anomalyDetectionAuthKey;
            _liveId = liveId;
            _useMarketApi = useMarketApi;

            _spikeDetectorParams = string.Format("SpikeDetector.TukeyThresh={0}; SpikeDetector.ZscoreThresh={1}", tukeyThresh, zscoreThresh);
        }


        public Task<AnomalyRecord[]> Analyze(SensorDataContract[] data)
        {
            var timeSeriesData = GetTimeseriesData(data);
#if DEBUG_LOG
            Trace.TraceInformation("AzureML request: {0}", timeSeriesData);
#endif

            if (_useMarketApi)
            {
                return Task.Run(() => GetAlertsFromAnomalyDetectionAPI(timeSeriesData));  
            }
            return GetAlertsFromRRS(timeSeriesData);
        }

        private static string GetTimeseriesData(SensorDataContract[] data)
        {
            var step = 1;
            var prevTime = DateTime.MinValue;
            var prevVal = 0d;
            List<SensorDataContract> newData = new List<SensorDataContract>();
            foreach (var d in data.OrderBy(dd => dd.TimeCreated))
            {
                d.TimeCreated = d.TimeCreated.AddTicks(-(d.TimeCreated.Ticks % TimeSpan.TicksPerSecond)); // round off the millisecs
                if (prevTime != DateTime.MinValue)
                {
                    for (; prevTime.AddSeconds(step) < d.TimeCreated; )
                    {
                        prevTime = prevTime.AddSeconds(step);
                        newData.Add(new SensorDataContract { TimeCreated = prevTime, Value = prevVal });
                    }
                }
                newData.Add(d);

                prevTime = d.TimeCreated;
                prevVal = d.Value;
            }


            var sb = new StringBuilder();
            foreach (var d in newData)
            {
                sb.Append(string.Format("{0}={1};", d.TimeCreated.ToString("O"), d.Value));
            }
            return sb.ToString();
        }

        public static double FindMaxValue(IEnumerable<double> values)
        {
            var avg = values.Average();
            var sd = Math.Sqrt(values.Average(v => Math.Pow(v - avg, 2)));
            return avg + 5 * sd;
        }

        private AnomalyRecord[] filterAnomaly(IEnumerable<AnomalyRecord> analyzedRecords)
        {
            return analyzedRecords.Where(ar => ar.Spike1 == 1 || ar.Spike2 == 1 || ar.LevelScore > 3).ToArray();
        }


        public AnomalyRecord[] GetAlertsFromAnomalyDetectionAPI(string timeSeriesData)
        {
            var acitionUri = new Uri(_detectorUrl);

            var cred = new NetworkCredential(_liveId, _detectorAuthKey); // your Microsoft live Id here 
            var cache = new CredentialCache();
            cache.Add(acitionUri, "Basic", cred);

            DataServiceContext ctx = new DataServiceContext(acitionUri);
            ctx.Credentials = cache;

            var query = ctx.Execute<ADResult>(acitionUri, "POST", true,
                            new BodyOperationParameter("data", timeSeriesData),
                            new BodyOperationParameter("params", _spikeDetectorParams));

            var resultTable = query.FirstOrDefault();
            var results = resultTable.GetADResults();

            var presults = results;
            return filterAnomaly(presults);
        }

        private Task<AnomalyRecord[]> GetAlertsFromRRS(string timeSeriesData)
        {
            var rrs = _detectorUrl;
            var apikey = _detectorAuthKey;

            using (var wb = new WebClient())
            {
                wb.Headers[HttpRequestHeader.ContentType] = "application/json";
                wb.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + apikey);

                var featureVector =
                    string.Format("\"data\": \"{0}\",\"params\": \"{1}\"", timeSeriesData, _spikeDetectorParams);
                string jsonData = "{\"Id\":\"scoring0001\", \"Instance\": {\"FeatureVector\": {" + featureVector + "}, \"GlobalParameters\":{\"level_mhist\": 300, \"level_shist\": 100, \"trend_mhist\": 300, \"trend_shist\": 100 }}}";
                var jsonBytes = Encoding.Default.GetBytes(jsonData);

                return wb.UploadDataTaskAsync(rrs, "POST", jsonBytes)
                    
                    .ContinueWith(
                    resp =>
                    {
                        var response = Encoding.Default.GetString(resp.Result);
#if DEBUG_LOG
                        Trace.TraceInformation("AzureML response: {0}...", response.Substring(0, Math.Min(100, response.Length)));
#endif

                        JavaScriptSerializer ser = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
                        var results = ser.Deserialize<List<string[]>>(response);

                        var presults = results.Select(AnomalyRecord.Parse);
                        return filterAnomaly(presults);
                    }
                    );
            }
        }
    }
}