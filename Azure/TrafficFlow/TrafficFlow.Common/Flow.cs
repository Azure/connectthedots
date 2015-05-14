using System;
using System.Runtime.Serialization;

namespace TrafficFlow.Common
{
    [Serializable]
    [DataContract]
    public class FlowStationLocation
    {
        [DataMember(Name = "Description")]
        public string Description;

        [DataMember(Name = "Direction")]
        public string Direction;

        [DataMember(Name = "Latitude")]
        public double Latitude;

        [DataMember(Name = "Longitude")]
        public double Longitude;

        [DataMember(Name = "MilePost")]
        public double MilePost;

        [DataMember(Name = "RoadName")]
        public string RoadName;
    }

    [Serializable]
    [DataContract]
    public class Flow
    {
        [DataMember(Name = "FlowDataID")]
        public int FlowDataID;

        [DataMember(Name = "FlowReadingValue")]
        public int FlowReadingValue;

        [DataMember(Name = "FlowStationLocation")]
        public FlowStationLocation FlowStationLocation;

        [DataMember(Name = "Region")]
        public string Region;

        [DataMember(Name = "StationName")]
        public string StationName;

        [DataMember(Name = "Time")]
        public DateTime Time;
    }
}
