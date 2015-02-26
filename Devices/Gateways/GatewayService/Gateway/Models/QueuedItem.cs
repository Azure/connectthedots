using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using SharedInterfaces;

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
        public static SensorDataContract SensorDataContractFromQueuedItem(QueuedItem data, ILogger logger = null)
        {
            SensorDataContract result = null;
            try
            {
                result =
                    JsonConvert.DeserializeObject<SensorDataContract>(data.JsonData);
            }
            catch (Exception ex)
            {
                //TODO: maybe better to add some metrics instead
                if (logger != null)
                {
                    logger.LogError("Error on deserialize queued item: " + ex.Message);
                }
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
