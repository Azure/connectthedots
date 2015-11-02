namespace ApiReaders
{
    using System;
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
        public RawXMLWithHeaderToJsonReader(bool useXML, string address, NetworkCredential credential)
        {
            _credential = credential;
            _initialApiAddress = address;
            _currentApiAddress = address;

            _useXML = useXML;
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
                                foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                                {
                                    if (node.Name == "message")
                                    {
                                        containsMessage = true;

                                        XmlAttribute att = doc.CreateAttribute("json", "Array",
                                            "http://james.newtonking.com/projects/json");
                                        att.Value = "true";

                                        if (node.Attributes != null)
                                        {
                                            node.Attributes.Append(att);
                                        }

                                        foreach (XmlNode messageNode in node.ChildNodes)
                                        {
                                            if (!messageNode.Name.EndsWith("Block")) continue;

                                            XmlAttribute att2 = doc.CreateAttribute("json", "Array",
                                            "http://james.newtonking.com/projects/json");
                                            att2.Value = "true";
                                            if (messageNode.Attributes != null)
                                            {
                                                messageNode.Attributes.Append(att2);
                                            }
                                        }
                                    }
                                    else if (node.Name == "nextBuffer")
                                    {
                                        foreach (XmlNode nextBufferNode in node.ChildNodes)
                                        {
                                            if (nextBufferNode.Name != "url") continue;

                                            bool changedAddress = false;

                                            foreach (XmlNode child in nextBufferNode.ChildNodes)
                                            {
                                                if (child.NodeType != XmlNodeType.Text &&
                                                    child.NodeType != XmlNodeType.CDATA) continue;

                                                var newAddressCandidate = child.Value;
                                                if (newAddressCandidate.Length > 0)
                                                {
                                                    _currentApiAddress = newAddressCandidate;
                                                }
                                                changedAddress = true;
                                                break;
                                            }
                                            if (changedAddress) break;
                                        }
                                    }
                                }
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
