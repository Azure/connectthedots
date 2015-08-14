using System.Collections.Generic;
using System.Text;
using System.Threading;
using Data.Contracts;
using Newtonsoft.Json;

namespace WebService
{
    public class ResponsesRTCache
    {
        private readonly DataCache _Cache = new DataCache();
        private byte[] _currentSerializedValues = new byte[0];
        private long _counter = 0, _currentVersion = 0;

        private static readonly ResponsesRTCache _ResponsesCache =
               new ResponsesRTCache();
        public static ResponsesRTCache Instance
        {
            get { return _ResponsesCache; }
        }

        private ResponsesRTCache() { }   

        public void Set(ApiDataContract data)
        {
            bool updateDataValue;
            bool updateDataSource;

            _Cache.Set(data, out updateDataValue, out updateDataSource);
            if (updateDataValue || updateDataSource)
            {
                Interlocked.Increment(ref _counter);
            }
        }

        public ApiDataContract GetValue(int id)
        {
            return _Cache.GetValue(id);
        }

        public void DeserializeAndSet(string jsonData)
        {
            try
            {
                EHRawDataContract ehData = JsonConvert.DeserializeObject<EHRawDataContract>(jsonData);
                ApiDataContract data = new ApiDataContract
                {
                    DataID = int.Parse(ehData.DataID),
                    ReadingValue = (int)ehData.Value,
                    StationLocation = new StationLocation
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

                Set(data);
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
                        var values = _Cache.GetValues();
                        _currentSerializedValues = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(values));
                    }
                }
            }

            return _currentSerializedValues;
        }
    }
}