using System.Runtime.Serialization;

namespace Gateway.Models
{
    [DataContract]
    public class SensorDataContract
    {
        //[DataMember(Name = "tagId")]
        //public int TagId { get; set; }

        //[DataMember(Name = "dataSourceName")]
        //public string DataSourceName { get; set; }

        //[DataMember(Name = "dataIntegrity")]
        //public int DataIntegrity { get; set; }

        [DataMember(Name = "Value")]
        public double Value { get; set; }

        //Dinar: temporary will send it as a string while self-describing data is not implemented 
        //[DataMember(Name = "serializedData")]
        //public string JsonData { get; set; }

        [DataMember(Name = "GUID")]
        public int Guid { get; set; }

        [DataMember(Name = "Organization")]
        public string Organization { get; set; }

        [DataMember(Name = "DisplayName")]
        public string DisplayName { get; set; }

        [DataMember(Name = "UnitOfMeasure")]
        public string UnitOfMeasure { get; set; }

        [DataMember(Name = "Measure")]
        public string Measure { get; set; }

        [DataMember(Name = "Location")]
        public string Location { get; set; }
    }
}
