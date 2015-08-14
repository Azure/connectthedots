using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Data.Contracts
{
    public class DataCache
    {
        private readonly ConcurrentDictionary<int, ApiDataContract> _data = new ConcurrentDictionary<int, ApiDataContract>();

        public void Set(ApiDataContract data, out bool updateDataValue, out bool updateDataSource)
        {
            const double LOCATION_EPS = 0.001;

            bool updateDataValueResult = false;
            bool updateDataSourceResult = false;

            if (!_data.ContainsKey(data.DataID))
            {
                updateDataValueResult = updateDataSourceResult = true;
            }

            _data.AddOrUpdate(data.DataID, data,
                (key, oldValue) =>
                {
                    if (oldValue.ReadingValue != data.ReadingValue)
                    {
                        updateDataValueResult = true;
                    }
                    if (oldValue.Region != data.Region
                        || oldValue.StationName != data.StationName
                        || oldValue.StationLocation.Description != data.StationLocation.Description
                        || oldValue.StationLocation.Direction != data.StationLocation.Direction
                        || Math.Abs(oldValue.StationLocation.MilePost - data.StationLocation.MilePost) > LOCATION_EPS
                        || oldValue.StationLocation.RoadName != data.StationLocation.RoadName
                        || Math.Abs(oldValue.StationLocation.Latitude - data.StationLocation.Latitude) > LOCATION_EPS
                        || Math.Abs(oldValue.StationLocation.Longitude - data.StationLocation.Longitude) > LOCATION_EPS)
                    {
                        updateDataSourceResult = true;
                    }
                    return data;
                });

            updateDataValue = updateDataValueResult;
            updateDataSource = updateDataSourceResult;
        }

        public void DeserializeAndSet(string jsonData)
        {
            try
            {
                ApiDataContract data = JsonConvert.DeserializeObject<ApiDataContract>(jsonData);

                bool updateValue;
                bool updateSource;
                Set(data, out updateValue, out updateSource);
            }
            catch { }
        }

        public ApiDataContract GetValue(int id)
        {
            ApiDataContract data;
            bool result = _data.TryGetValue(id, out data);
            return result ? data : null;
        }
        public IEnumerable<ApiDataContract> GetValues()
        {
            return _data.Values;
        }
    }
}