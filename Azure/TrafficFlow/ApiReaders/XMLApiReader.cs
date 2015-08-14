using System.Runtime.Serialization;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;

namespace ApiReaders
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;

    public interface IEntity<out T>
    {
        T Id();
    }
    public class XMLApiReaderProcess<TDataSourceKey, TDefinition, TData>
        where TDefinition : IEntity<TDataSourceKey>
        where TData : IEntity<TDataSourceKey>
    {
        private static string _DefinitionAddress;
        private static string _DataAddress;

        private static string _DefinitionXMLRootNodeName;
        private static string _DataXMLRootNodeName;

        private static int _IntervalSecs;

        private static readonly Dictionary<TDataSourceKey, TDefinition> _Definitions
            = new Dictionary<TDataSourceKey, TDefinition>();
        private readonly Action<TDefinition, TData> _OnData;

        public XMLApiReaderProcess(string definitionAddress, string definitionXMLRootNodeName,
            string dataAddress, string dataXMLRootNodeName,
            int intervalSecs, Action<TDefinition, TData> onData)
        {
            _DefinitionAddress = definitionAddress;
            _DataAddress = dataAddress;

            _DefinitionXMLRootNodeName = definitionXMLRootNodeName;
            _DataXMLRootNodeName = dataXMLRootNodeName;

            _IntervalSecs = intervalSecs;
            _OnData = onData;
        }

        public void Start()
        {
            TDefinition[] def = GetDefinition();
            foreach (TDefinition definition in def)
            {
                _Definitions.Add(definition.Id(), definition);
            }

            int sleepMS = _IntervalSecs * 1000;
            for (; ; )
            {
                try
                {
                    TData[] dataItems = GetData();

                    foreach (TData data in dataItems)
                    {
                        try
                        {
                            TDefinition definitionForData = _Definitions[data.Id()];
                            _OnData(definitionForData, data);
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                    Thread.Sleep(sleepMS);
                }
                catch (Exception ex)
                {
                }
            }
        }

        public TDefinition[] GetDefinition()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_DefinitionAddress);
            request.Method = "GET";

            TDefinition[] result = null;

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
                            XmlSerializer ser = new XmlSerializer(typeof (TDefinition[]),
                                new XmlRootAttribute(_DefinitionXMLRootNodeName));
                            result = (TDefinition[])ser.Deserialize(responseStream);
                        }
                        catch (Exception e)
                        {
                        }
                    }
                }
            }

            return result;
        }

        public TData[] GetData()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_DataAddress);
            request.Method = "GET";

            TData[] result = null;

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
                            XmlSerializer ser = new XmlSerializer(typeof(TData[]), new XmlRootAttribute(_DataXMLRootNodeName));
                            result = (TData[])ser.Deserialize(responseStream);
                        }
                        catch (Exception e)
                        {
                        }
                    }
                }
            }
            return result;
        }
    }
}
