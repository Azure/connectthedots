namespace ApiReaders
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    

    public class ApiReader
    {
        private static string _url;
        public ApiReader(string url)
        {
            _url = url;
        }

        public async Task<IList<T>> GetTrafficFlowsAsJson<T>()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_url);
            request.Method = "GET";

            IList<T> messagePayloads = null;

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
                                JsonConvert.DeserializeObject<IList<T>>(responseJSON);
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
