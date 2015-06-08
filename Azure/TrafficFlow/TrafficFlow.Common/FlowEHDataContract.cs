using System;
using System.Runtime.Serialization;

namespace TrafficFlow.Common
{
    [DataContract]
    public class FlowEHDataContract
    {
        [DataMember(Name = "FlowDataID")]
        public string FlowDataID { get; set; }

        [DataMember(Name = "Region")]
        public string Region { get; set; }

        [DataMember(Name = "StationName")]
        public string StationName { get; set; }

        [DataMember(Name = "LocationDescription")]
        public string LocationDescription { get; set; }

        [DataMember(Name = "Direction")]
        public string Direction { get; set; }

        [DataMember(Name = "Latitude")]
        public string Latitude { get; set; }

        [DataMember(Name = "Longitude")]
        public string Longitude { get; set; }

        [DataMember(Name = "MilePost")]
        public string MilePost { get; set; }

        [DataMember(Name = "RoadName")]
        public string RoadName { get; set; }

        [DataMember(Name = "value")]
        public double Value { get; set; }

        [DataMember(Name = "TimeCreated")]
        public DateTime TimeCreated { get; set; }
    }
}
