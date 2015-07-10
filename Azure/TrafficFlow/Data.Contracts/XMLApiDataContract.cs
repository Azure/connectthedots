using System.Xml.Serialization;
using ApiReaders;

namespace Data.Contracts
{
    [XmlType("station")]
    [XmlRoot("station")]
    public class XMLApiDefinition : IEntity<string>
    {
        [XmlAttribute]
        public string id;

        [XmlElement("route")]
        public string Route;
        [XmlElement("direction")]
        public string Direction;
        [XmlElement("milepost")]
        public string Milepost;
        [XmlElement("location")]
        public string Location;

        public string Id()
        {
            return id;
        }
    }

    [XmlType("station")]
    [XmlRoot("station")]
    public class XMLApiData : IEntity<string>
    {
        [XmlAttribute]
        public string id;

        [XmlAttribute("stat")]
        public string Status;

        [XmlElement("vol")]
        public decimal Volume;
        [XmlElement("occ")]
        public decimal Occupancy;
        [XmlElement("spd")]
        public decimal Speed;

        public string Id()
        {
            return id;
        }
    }
}
