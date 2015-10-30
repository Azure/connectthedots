using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel.Web;
using System.Text;
using Data.Contracts;
using Data.Repositories;
using Microsoft.WindowsAzure;
using Newtonsoft.Json;

namespace WebService
{
    public class DataService : IDataService
    {
        private static readonly ResponsesRTCache _cache = ResponsesRTCache.Instance;
        private static readonly DataCache _sourcesCache = new DataCache();

        private static DataValueRepository _FlowDataRepository;
        private static DataSourceRepository _FlowSourcesRepository;

        static DataService()
        {
            //Task.Run(()=>Init());
            Init();
        }

        private static void Init()
        {
            var ehName = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.EventHubDevices").ToLowerInvariant();
            var namespaceManager = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");
            var consumerGroup = String.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME")) ? "local" : "WebSite";

            string sqlDatabaseConnectionString = CloudConfigurationManager.GetSetting("sqlDatabaseConnectionString");
            _FlowDataRepository = new DataValueRepository(sqlDatabaseConnectionString);
            _FlowSourcesRepository = new DataSourceRepository(sqlDatabaseConnectionString);

            UpdateSourcesCache();

            var ehReader = new EventHubReader(consumerGroup);
            ehReader.Run(namespaceManager, ehName, (m) => { _cache.DeserializeAndSet(m); _sourcesCache.DeserializeAndSet(m); });
        }

        
        public Stream GetData()
        {
            SetContentTypeToJson();

            return new MemoryStream(_cache.GetSerializedValues());
        }

        public Stream GetDataHistorical(DateTime start, DateTime end)
        {
            SetContentTypeToJson();

            IList<ApiDataContract> dataList = _FlowDataRepository.QueryByDateInterval(start, end);
            foreach (var data in dataList)
            {
                var cachedData = _sourcesCache.GetValue(data.DataID) ?? _cache.GetValue(data.DataID);

                if (cachedData == null)
                {
                    //very few calls could be performed from here
                    UpdateSourcesCache();
                }
            }

            var resultList = new List<ApiDataContract>();
            foreach (var data in dataList)
            {
                var cachedData = _sourcesCache.GetValue(data.DataID) ?? _cache.GetValue(data.DataID);

                if (cachedData != null && cachedData.StationLocation != null)
                {
                    var valueToAdd = new ApiDataContract
                    {
                        DataID = data.DataID,
                        ReadingValue = data.ReadingValue,
                        Time = data.Time,
                        Region = cachedData.Region,
                        StationName = cachedData.StationName,
                        StationLocation = new StationLocation
                        {
                            Description = cachedData.StationLocation.Description,
                            Direction = cachedData.StationLocation.Direction,
                            Latitude = cachedData.StationLocation.Latitude,
                            Longitude = cachedData.StationLocation.Longitude,
                            MilePost = cachedData.StationLocation.MilePost,
                            RoadName = cachedData.StationLocation.RoadName
                        }
                    };
                    resultList.Add(valueToAdd);
                }
            }
            return new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(resultList)));
        }

        private void SetContentTypeToJson()
        {
            if (WebOperationContext.Current != null)
            {
                WebOperationContext.Current.OutgoingResponse.ContentType =
                    "application/json; charset=utf-8";
            }
        }

        private static void UpdateSourcesCache()
        {
            IList<ApiDataContract> sourcesList = _FlowSourcesRepository.FetchAll();
            foreach (var flow in sourcesList)
            {
                bool updateFlowValue;
                bool updateFlowSource;
                _sourcesCache.Set(flow, out updateFlowValue, out updateFlowSource);
            }
        }

    }
}
