namespace ApiReaders
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Xml;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

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
                            
                            string jsonText = JsonConvert.SerializeXmlNode(doc);

                            var messageDictionary = (IDictionary<string, object>)
                                JsonConvert.DeserializeObject(jsonText, typeof(IDictionary<string, object>));
                            var dataContent = (JObject)messageDictionary.First().Value;

                            bool notEmpty = false;

                            foreach (var pair in dataContent)
                            {
                                var key = pair.Key;
                                if (key.Contains("message"))
                                {
                                    notEmpty = true;
                                }
                                if (key.Contains("nextBuffer"))
                                {
                                    var nextBufferContent = (JObject)pair.Value;
                                    foreach (var nextBufferPair in nextBufferContent)
                                    {
                                        if (nextBufferPair.Key == "url")
                                        {
                                            var newAddressCandidate = (string)nextBufferPair.Value;
                                            if (newAddressCandidate.Length > 0)
                                            {
                                                _currentApiAddress = newAddressCandidate;
                                            }
                                        }
                                    }
                                }
                            }
                            if (notEmpty)
                            {
                                return _useXML ? originalText : jsonText;
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
