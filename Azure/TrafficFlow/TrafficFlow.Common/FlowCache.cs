using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace TrafficFlow.Common
{
    public class FlowCache
    {
        private readonly ConcurrentDictionary<int, Flow> _data = new ConcurrentDictionary<int, Flow>();

        public void Set(Flow flow, out bool updateFlowValue, out bool updateFlowSource)
        {
            const double LOCATION_EPS = 0.001;

            bool updateFlowValueResult = false;
            bool updateFlowSourceResult = false;

            if (!_data.ContainsKey(flow.FlowDataID))
            {
                updateFlowValueResult = updateFlowSourceResult = true;
            }

            _data.AddOrUpdate(flow.FlowDataID, flow,
                (key, oldValue) =>
                {
                    if (oldValue.FlowReadingValue != flow.FlowReadingValue)
                    {
                        updateFlowValueResult = true;
                    }
                    if (oldValue.Region != flow.Region
                        || oldValue.StationName != flow.StationName
                        || oldValue.FlowStationLocation.Description != flow.FlowStationLocation.Description
                        || oldValue.FlowStationLocation.Direction != flow.FlowStationLocation.Direction
                        || Math.Abs(oldValue.FlowStationLocation.MilePost - flow.FlowStationLocation.MilePost) > LOCATION_EPS
                        || oldValue.FlowStationLocation.RoadName != flow.FlowStationLocation.RoadName
                        || Math.Abs(oldValue.FlowStationLocation.Latitude - flow.FlowStationLocation.Latitude) > LOCATION_EPS
                        || Math.Abs(oldValue.FlowStationLocation.Longitude - flow.FlowStationLocation.Longitude) > LOCATION_EPS)
                    {
                        updateFlowSourceResult = true;
                    }
                    return flow;
                });

            updateFlowValue = updateFlowValueResult;
            updateFlowSource = updateFlowSourceResult;
        }

        public void DeserializeAndSet(string jsonFlow)
        {
            try
            {
                Flow flow = JsonConvert.DeserializeObject<Flow>(jsonFlow);

                bool updateFlowValue;
                bool updateFlowSource;
                Set(flow, out updateFlowValue, out updateFlowSource);
            }
            catch { }
        }

        public Flow GetValue(int id)
        {
            Flow flow;
            bool result = _data.TryGetValue(id, out flow);
            return result ? flow : null;
        }
        public IEnumerable<Flow> GetValues()
        {
            return _data.Values;
        }
    }
}