using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Data.Contracts
{
    [DataContract]
    public class EHRawXMLApiDataContract
    {
        [DataMember(Name = "StationID")]
        public string StationID { get; set; }

        [DataMember(Name = "Route")]
        public string Route { get; set; }

        [DataMember(Name = "Direction")]
        public string Direction { get; set; }

        [DataMember(Name = "MilePost")]
        public string Milepost { get; set; }

        [DataMember(Name = "Location")]
        public string Location { get; set; }

        [DataMember(Name = "Volume")]
        public decimal Volume;

        [DataMember(Name = "Occupancy")]
        public decimal Occupancy;

        [DataMember(Name = "Speed")]
        public decimal Speed;

        [DataMember(Name = "TimeCreated")]
        public DateTime TimeCreated { get; set; }
    }

}
