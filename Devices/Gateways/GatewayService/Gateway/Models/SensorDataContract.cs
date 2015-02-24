using System;
using System.Runtime.Serialization;

namespace Gateway.Models
{
    [DataContract]
    public class SensorDataContract
    {
        [DataMember(Name = "Value")]
        public double Value { get; set; }

        [DataMember(Name = "GUID")]
        public int Guid { get; set; }

        [DataMember(Name = "Organization")]
        public string Organization { get; set; }

        [DataMember(Name = "DisplayName")]
        public string DisplayName { get; set; }

        [DataMember(Name = "UnitOfMeasure")]
        public string UnitOfMeasure { get; set; }

        [DataMember(Name = "MeasureName")]
        public string MeasureName { get; set; }

        [DataMember(Name = "Location")]
        public string Location { get; set; }

        [DataMember(Name = "time_created")]
        public DateTime TimeCreated { get; set; }
    }
}
