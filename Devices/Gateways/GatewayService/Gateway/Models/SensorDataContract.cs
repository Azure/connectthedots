using System.Runtime.Serialization;

namespace Gateway.Models
{
    [DataContract]
    public class SensorDataContract
    {
        [DataMember(Name = "tagId")]
        public int TagId { get; set; }

        [DataMember(Name = "dataSourceName")]
        public string DataSourceName { get; set; }

        [DataMember(Name = "dataIntegrity")]
        public int DataIntegrity { get; set; }

        [DataMember(Name = "value")]
        public double Value { get; set; }

        //Dinar: temporary will send it as a string while self-describing data is not implemented 
        [DataMember(Name = "serializedData")]
        public string JsonData { get; set; }
    }
}
