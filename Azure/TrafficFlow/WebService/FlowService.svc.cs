using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.PeerResolvers;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;
using Microsoft.WindowsAzure;
using Newtonsoft.Json;
using TrafficFlow.Common;
using TrafficFlow.Common.Repositories;

namespace WebService
{
    public class FlowService : IFlowService
    {
        private static readonly FlowResponsesRTCache _cache = FlowResponsesRTCache.Instance;
        private static readonly FlowCache _sourcesCache = new FlowCache();

        private static FlowDataRepository _FlowDataRepository;
        private static FlowSourcesRepository _FlowSourcesRepository;

        static FlowService()
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
            _FlowDataRepository = new FlowDataRepository(sqlDatabaseConnectionString);
            _FlowSourcesRepository = new FlowSourcesRepository(sqlDatabaseConnectionString);

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

            IList<Flow> dataList = _FlowDataRepository.QueryByDateInterval(start, end);
            foreach (var data in dataList)
            {
                var cachedData = _sourcesCache.GetValue(data.FlowDataID) ?? _cache.GetValue(data.FlowDataID);

                if (cachedData == null)
                {
                    //very few calls could be performed from here
                    UpdateSourcesCache();
                }
            }

            var resultList = new List<Flow>();
            foreach (var data in dataList)
            {
                var cachedData = _sourcesCache.GetValue(data.FlowDataID) ?? _cache.GetValue(data.FlowDataID);

                if (cachedData != null && cachedData.FlowStationLocation != null)
                {
                    var valueToAdd = new Flow
                    {
                        FlowDataID = data.FlowDataID,
                        FlowReadingValue = data.FlowReadingValue,
                        Time = data.Time,
                        Region = cachedData.Region,
                        StationName = cachedData.StationName,
                        FlowStationLocation = new FlowStationLocation
                        {
                            Description = cachedData.FlowStationLocation.Description,
                            Direction = cachedData.FlowStationLocation.Direction,
                            Latitude = cachedData.FlowStationLocation.Latitude,
                            Longitude = cachedData.FlowStationLocation.Longitude,
                            MilePost = cachedData.FlowStationLocation.MilePost,
                            RoadName = cachedData.FlowStationLocation.RoadName
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
            IList<Flow> sourcesList = _FlowSourcesRepository.FetchAll();
            foreach (var flow in sourcesList)
            {
                bool updateFlowValue;
                bool updateFlowSource;
                _sourcesCache.Set(flow, out updateFlowValue, out updateFlowSource);
            }
        }

    }
}
