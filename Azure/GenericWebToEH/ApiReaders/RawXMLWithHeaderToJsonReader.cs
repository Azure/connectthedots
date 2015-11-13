namespace ApiReaders
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Xml;
    using Newtonsoft.Json;

    public class RawXMLWithHeaderToJsonReader
    {
        private readonly string _initialApiAddress;
        private string _currentApiAddress;

        private readonly bool _useXML;
        private readonly NetworkCredential _credential;

        class AttributesMap : Dictionary<string, KeyValuePair<IList<XmlAttribute>, AttributesMap>> { }

        private readonly AttributesMap _templateAttributesMap;

        public RawXMLWithHeaderToJsonReader(string xmlTemplate, bool useXML, string address, NetworkCredential credential)
        {
            _templateAttributesMap = PrepareAttributesMap(xmlTemplate);

            _credential = credential;
            _initialApiAddress = address;
            _currentApiAddress = address;

            _useXML = useXML;
        }

        private AttributesMap PrepareAttributesMap(string xmlText)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlText);
            AttributesMap result = ParseAttributesForXmlNode(doc.DocumentElement);
            return result;
        }

        private AttributesMap ParseAttributesForXmlNode(XmlNode rootNode)
        {
            if (rootNode == null)
            {
                return null;
            }

            AttributesMap result = new AttributesMap();

            foreach (XmlNode node in rootNode)
            {
                IList<XmlAttribute> list = new List<XmlAttribute>();
                if (node.Attributes != null)
                {
                    foreach (XmlAttribute attribute in node.Attributes)
                    {
                        list.Add(attribute);
                    }
                }

                result.Add(node.Name, new KeyValuePair<IList<XmlAttribute>, AttributesMap>(list, ParseAttributesForXmlNode(node)));
            }

            return result;
        }

        private void ApplyTemplateAttributes(XmlNode rootNode, AttributesMap templateMap, Action<XmlNode, XmlAttribute> applyAttributeToNode)
        {
            if (rootNode == null)
                return;

            foreach (XmlNode node in rootNode)
            {
                if (!templateMap.ContainsKey(node.Name)) continue;

                foreach (XmlAttribute attributeInTemplate in templateMap[node.Name].Key)
                {
                    applyAttributeToNode(node, attributeInTemplate);
                }
                ApplyTemplateAttributes(node, templateMap[node.Name].Value, applyAttributeToNode);
            }
        }


        public string GetData()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_currentApiAddress);
            request.Method = "GET";
            request.Credentials = _credential;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                if (response.StatusCode != HttpStatusCode.OK)
                {
                }
                else
                {
                    Stream responseStream = response.GetResponseStream();
                    if (responseStream != null)
                    {
                        try
                        {
                            StreamReader reader = new StreamReader(responseStream);
                            string originalText = reader.ReadToEnd();

                            XmlDocument doc = new XmlDocument();
                            doc.LoadXml(originalText);

                            bool containsMessage = false;

                            if (doc.DocumentElement != null)
                            {
                                ApplyTemplateAttributes(doc.DocumentElement, _templateAttributesMap,
                                    (node, attributeInTemplate) =>
                                    {
                                        switch (attributeInTemplate.LocalName)
                                        {
                                            case "ctdNextBuffer":
                                                {
                                                    foreach (XmlNode child in node.ChildNodes)
                                                    {
                                                        if (child.NodeType != XmlNodeType.Text &&
                                                            child.NodeType != XmlNodeType.CDATA) continue;

                                                        var newAddressCandidate = child.Value;
                                                        if (newAddressCandidate.Length > 0)
                                                        {
                                                            _currentApiAddress = newAddressCandidate;
                                                        }
                                                        break;
                                                    }
                                                }
                                                break;
                                            case "ctdMessageMarker":
                                                {
                                                    containsMessage = true;
                                                }
                                                break;
                                            default:
                                                {
                                                    if (node.Attributes == null) return;

                                                    XmlAttribute att = doc.CreateAttribute(
                                                        attributeInTemplate.Prefix,
                                                        attributeInTemplate.LocalName,
                                                        attributeInTemplate.NamespaceURI
                                                        );

                                                    att.Value = attributeInTemplate.Value;
                                                    node.Attributes.Append(att);
                                                }
                                                break;
                                        }
                                    });
                            }
                            
                            if (containsMessage)
                            {
                                string result = _useXML ? originalText : JsonConvert.SerializeXmlNode(doc);
                                return result;
                            }
                        }
                        catch (Exception e)
                        {
                        }
                    }
                }
            }
            return null;
        }
    }
}
