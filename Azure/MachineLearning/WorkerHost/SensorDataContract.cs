using System;
using System.Runtime.Serialization;

namespace WorkerHost
{
    public class SensorDataContract
    {
        [DataMember(Name = "value")]
        public double Value { get; set; }

        [DataMember(Name = "guid")]
        public string Guid { get; set; }

        [DataMember(Name = "organization")]
        public string Organization { get; set; }

        [DataMember(Name = "displayname")]
        public string DisplayName { get; set; }

        [DataMember(Name = "unitofmeasure")]
        public string UnitOfMeasure { get; set; }

        [DataMember(Name = "measurename")]
        public string MeasureName { get; set; }

        [DataMember(Name = "location")]
        public string Location { get; set; }

        [DataMember(Name = "timecreated")]
        public DateTime TimeCreated { get; set; }

        public string UniqueId()
        {
            //we could have devices with same DisplayName but different MeasureName etc..
            return DisplayName + Guid + MeasureName;
        }
    }
}
