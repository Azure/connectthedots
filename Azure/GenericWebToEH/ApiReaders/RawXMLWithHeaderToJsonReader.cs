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

        private readonly bool _outputAsXml;
        private readonly bool _useMessageMarker;

        private readonly NetworkCredential _credential;

        private const string MESSAGE_MARKER_ATTRIBUTE_NAME = "ctdMessageMarker";
        private const string NEXT_BUFFER_ATTRIBUTE_NAME = "ctdNextBuffer";

        class AttributesMap : Dictionary<string, KeyValuePair<IList<XmlAttribute>, AttributesMap>> { }

        private readonly AttributesMap _templateAttributesMap;

        public RawXMLWithHeaderToJsonReader(string xmlTemplate, bool outputAsXml, string address, NetworkCredential credential)
        {
            _templateAttributesMap = PrepareAttributesMap(xmlTemplate, out _useMessageMarker);

            _credential = credential;
            _initialApiAddress = address;
            _currentApiAddress = address;

            _outputAsXml = outputAsXml;
        }

        public IEnumerable<string> GetData()
        {
            List<string> resultList = new List<string>();
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

                            IEnumerable<XmlDocument> documents = GetXmlFromOriginalText(originalText);

                            foreach (XmlDocument document in documents)
                            {
                                string resultString = ProcessDocument(document);
                                if (resultString != null)
                                {
                                    resultList.Add(resultString);
                                }
                            }
                        }
                        catch (Exception)
                        {
                            //error in reading
                        }
                    }
                }
            }
            return resultList;
        }

        private string ProcessDocument(XmlDocument document)
        {
            try
            {
                bool containsMessage = false;

                if (document.DocumentElement != null)
                {
                    ApplyTemplateAttributes(document.DocumentElement, _templateAttributesMap,
                        (node, attributeInTemplate) =>
                        {
                            switch (attributeInTemplate.LocalName)
                            {
                                case NEXT_BUFFER_ATTRIBUTE_NAME:
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
                                case MESSAGE_MARKER_ATTRIBUTE_NAME:
                                {
                                    containsMessage = true;
                                }
                                    break;
                                default:
                                {
                                    if (node.Attributes == null) return;

                                    XmlAttribute att = document.CreateAttribute(
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

                if (!_useMessageMarker || containsMessage)
                {
                    string result;

                    if (_outputAsXml)
                    {
                        using (var stringWriter = new StringWriter())
                        using (var xmlTextWriter = XmlWriter.Create(stringWriter))
                        {
                            document.WriteTo(xmlTextWriter);
                            xmlTextWriter.Flush();
                            result = stringWriter.GetStringBuilder().ToString();
                        }
                    }
                    else
                    {
                        result = JsonConvert.SerializeXmlNode(document, Newtonsoft.Json.Formatting.Indented);
                    }

                    return result;
                }
            }
            catch (Exception)
            {
                //ignored
            }
            return null;
        }

        private AttributesMap PrepareAttributesMap(string xmlText, out bool useMessageMarker)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlText);
            AttributesMap result = ParseAttributesForXmlNode(doc.DocumentElement, out useMessageMarker);
            return result;
        }

        private AttributesMap ParseAttributesForXmlNode(XmlNode rootNode, out bool useMessageMarker)
        {
            useMessageMarker = false;
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
                        if (attribute.LocalName == MESSAGE_MARKER_ATTRIBUTE_NAME)
                        {
                            useMessageMarker = true;
                        }
                        list.Add(attribute);
                    }
                }

                bool useMessageMarkerInChild;
                result.Add(node.Name, new KeyValuePair<IList<XmlAttribute>, AttributesMap>(list, ParseAttributesForXmlNode(node, out useMessageMarkerInChild)));
                useMessageMarker |= useMessageMarkerInChild;
            }

            return result;
        }

        private void ApplyTemplateAttributes(XmlNode rootNode, AttributesMap templateMap, Action<XmlNode, XmlAttribute> applyAttributeToNode)
        {
            if (rootNode == null)
            {
                return;
            }

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

        private IEnumerable<XmlDocument> GetXmlFromOriginalText(string originalText)
        {
            List<XmlDocument> result = new List<XmlDocument>();
            
            try
            {
                XmlDocument resultItem = new XmlDocument();
                resultItem.LoadXml(originalText);
                result.Add(resultItem);
                return result;
            }
            catch (Exception)
            {
            }

            try
            {
                XmlDocument resultItem = JsonConvert.DeserializeXmlNode(originalText);
                result.Add(resultItem);
                return result;
            }
            catch (Exception)
            {
            }

            XmlDocument resultArrayXml = JsonConvert.DeserializeXmlNode("{\"jsonRootNode\":" + originalText + "}", "Json");
            foreach (XmlNode jsonBlock in resultArrayXml)
            {
                foreach (XmlNode childNode in jsonBlock)
                {
                    try
                    {
                        XmlDocument nodeDoc = new XmlDocument();
                        nodeDoc.LoadXml(childNode.OuterXml);
                        result.Add(nodeDoc);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            return result;
        }
    }
}
