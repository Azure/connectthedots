using System;
using System.Runtime.Serialization;

namespace Data.Contracts
{
    [Serializable]
    [DataContract]
    public class StationLocation
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
    public class ApiDataContract
    {
        [DataMember(Name = "FlowDataID")]
        public int DataID;

        [DataMember(Name = "FlowReadingValue")]
        public int ReadingValue;

        [DataMember(Name = "FlowStationLocation")]
        public StationLocation StationLocation;

        [DataMember(Name = "Region")]
        public string Region;

        [DataMember(Name = "StationName")]
        public string StationName;

        [DataMember(Name = "Time")]
        public DateTime Time;
    }
}
