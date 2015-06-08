using System.Collections.Generic;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using TrafficFlow.Common;

namespace WebService
{
    public class FlowResponsesRTCache
    {
        private readonly FlowCache _FlowCache = new FlowCache();
        private byte[] _currentSerializedValues = new byte[0];
        private long _counter = 0, _currentVersion = 0;

        private static readonly FlowResponsesRTCache _FlowDataCache =
               new FlowResponsesRTCache();
        public static FlowResponsesRTCache Instance
        {
            get { return _FlowDataCache; }
        }

        private FlowResponsesRTCache() { }   

        public void Set(Flow flow)
        {
            bool updateFlowValue;
            bool updateFlowSource;

            _FlowCache.Set(flow, out updateFlowValue, out updateFlowSource);
            if (updateFlowValue || updateFlowSource)
            {
                Interlocked.Increment(ref _counter);
            }
        }

        public Flow GetValue(int id)
        {
            return _FlowCache.GetValue(id);
        }

        public void DeserializeAndSet(string jsonFlow)
        {
            try
            {
                FlowEHDataContract ehData = JsonConvert.DeserializeObject<FlowEHDataContract>(jsonFlow);
                Flow flow = new Flow
                {
                    FlowDataID = int.Parse(ehData.FlowDataID),
                    FlowReadingValue = (int)ehData.Value,
                    FlowStationLocation = new FlowStationLocation
                    {
                        Description = ehData.LocationDescription,
                        Direction = ehData.Direction,
                        Latitude = double.Parse(ehData.Latitude),
                        Longitude = double.Parse(ehData.Longitude),
                        MilePost = double.Parse(ehData.MilePost),
                        RoadName = ehData.RoadName
                    },
                    Region = ehData.Region,
                    StationName = ehData.StationName,
                    Time = ehData.TimeCreated
                };
                
                Set(flow);
            }
            catch { }
        }

        public byte[] GetSerializedValues()
        {
            long counterValue = Interlocked.Read(ref _counter);

            if (_currentVersion < counterValue)
            {
                lock (_currentSerializedValues)
                {
                    counterValue = Interlocked.Read(ref _counter);

                    if (_currentVersion < counterValue)
                    {
                        _currentVersion = counterValue;
                        var values = _FlowCache.GetValues();
                        _currentSerializedValues = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(values));
                    }
                }
            }

            return _currentSerializedValues;
        }
    }
}