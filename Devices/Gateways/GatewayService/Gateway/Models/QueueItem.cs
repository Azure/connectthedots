using System.Runtime.Serialization;

namespace Gateway.Models
{
    [DataContract]
    public class QueuedItem
    {
        [DataMember(Name = "serializedData")]
        public string JsonData { get; set; }
    }
}
