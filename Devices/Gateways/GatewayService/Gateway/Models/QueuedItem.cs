using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Gateway.Models
{
    [DataContract]
    public class QueuedItem
    {
        [DataMember(Name = "serializedData")]
        public string JsonData { get; set; }
    }

    public static class DataTransforms
    {
        public static SensorDataContract SensorDataContractFromQueuedItem(QueuedItem data)
        {
            SensorDataContract result = null;
            try
            {
                result =
                    JsonConvert.DeserializeObject<SensorDataContract>(data.JsonData);
            }
            catch (Exception /*ex*/)
            {
            }

            return result;
        }

        public static SensorDataContract AddTimeCreated(SensorDataContract data)
        {
            SensorDataContract result = data;
            if (result.TimeCreated == default(DateTime))
            {
                var creationTime = DateTime.UtcNow;
                result.TimeCreated = creationTime;
            }

            return result;
        }
    }
}
