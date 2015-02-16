using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Gateway;
using Gateway.Models;
using Gateway.Utils.Logger;
using Gateway.Utils.MessageSender;
using Gateway.Utils.Queue;
using CoreTest.Utils.Logger;
using System.Net;
using System.IO;
using System.Diagnostics;


namespace CoreTest
{
    public class WebServiceTest : ITest
    {
        public const int TEST_ITERATIONS = 100;
        public const int MAX_TEST_MESSAGES = 1000;

        string _url;
        private readonly ILogger _testLogger = new TestLogger();
        private readonly Random _rand;
        private int _totalMessages;
        
        private const int MINUTES_TO_MILLISECONDS = 60 * 1000;
        private const int STOP_TIMEOUT_MS = 5000; // ms
        private const int MIN_WAIT_BEETWEEN_BURSTS = 5 * MINUTES_TO_MILLISECONDS; // 5 minutes in milliseconds

        public WebServiceTest(string url)
        {
            _url = url;
            _rand = new Random();
            _totalMessages = 0;
        }

        public void Run()
        {
            try
            {
                // Send a flurry of messages, repeat a few times
                for (int iteration = 0; iteration < TEST_ITERATIONS; ++iteration)
                {
                    int count = _rand.Next(MAX_TEST_MESSAGES);

                    while (--count >= 0)
                    {
                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_url);
                        request.Method = "GET";

                        using(HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                        { 
                            if(response.StatusCode != HttpStatusCode.OK) 
                            {
                                SignalError( response.StatusCode );
                            }
                            else
                            {
                                _totalMessages++;
                            }
                        }
                    }

                    // sleep 5 to 10 minutes
                    int sleepMS = MIN_WAIT_BEETWEEN_BURSTS + _rand.Next(MIN_WAIT_BEETWEEN_BURSTS);

                    Console.WriteLine(String.Format("Sent {0} messages, sleeping now for {1} minutes", _totalMessages, sleepMS / MINUTES_TO_MILLISECONDS));

                    Thread.Sleep(sleepMS);
                }
            }
            catch (Exception ex)
            {
                _testLogger.LogError("exception caught: " + ex.StackTrace);
            }
        }

        public void Completed()
        {
            throw new NotImplementedException();
        }

        public int TotalMessagesSent
        {
            get
            {
                return _totalMessages;
            }
        }

        protected void SignalError( HttpStatusCode code )
        {
            _testLogger.LogError("Response yielded error: " + code.ToString());

            Debug.Assert(false);
        }

    }
}
