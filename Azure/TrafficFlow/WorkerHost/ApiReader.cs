namespace WorkerHost
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Runtime.Serialization.Json;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.Script.Serialization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using TrafficFlow.Common;

    

    public class ApiReader
    {
        private static string _url;
        public ApiReader(string url)
        {
            _url = url;
        }

        public async Task<IList<Flow>> GetTrafficFlowsAsJson()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_url);
            request.Method = "GET";

            IList<Flow> messagePayloads = null;

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
                        StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                        string responseJSON = reader.ReadToEnd();

                        try
                        {
                            messagePayloads =
                                JsonConvert.DeserializeObject<IList<Flow>>(responseJSON);
                        }
                        catch (Exception e)
                        {
                        }
                    }
                }
            }
            return messagePayloads;
        }
    }
}
