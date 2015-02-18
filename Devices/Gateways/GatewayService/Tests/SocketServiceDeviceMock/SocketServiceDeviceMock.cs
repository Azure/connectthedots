using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Gateway.Models;
using CoreTest.Utils.Generators;
using Newtonsoft.Json;

namespace SocketServiceDeviceMock
{
    class SocketServiceDeviceMock
    {
        private const int SLEEP_TIME_MS = 1000;

        static void Main(string[] args)
        {
            IPAddress ipAddress = new IPAddress(new byte[] { 192, 168, 50, 62 });//ipHostInfo.AddressList[0]);
            TcpListener serverSocket = new TcpListener(ipAddress, 5000);
            serverSocket.Start();

            TcpClient clientSocket = serverSocket.AcceptTcpClient();
            Console.WriteLine("Accepted connection from client.");

            try {
                for (;;)
                {
                    NetworkStream networkStream = clientSocket.GetStream();

                    //byte[] bytesFrom = new byte[10025];
                    //networkStream.Read(bytesFrom, 0, clientSocket.ReceiveBufferSize);

                    //string dataFromClient = Encoding.ASCII.GetString(bytesFrom);
                    //dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"));

                    SensorDataContract sensorData = RandomSensorDataGenerator.Generate();
                    string serializedData = JsonConvert.SerializeObject(sensorData);

                    Byte[] sendBytes = Encoding.ASCII.GetBytes("<" + serializedData + ">");

                    networkStream.Write(sendBytes, 0, sendBytes.Length);
                    networkStream.Flush();

                    Console.WriteLine("Sent: " + serializedData);
                    Thread.Sleep(SLEEP_TIME_MS);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            try
            {
                serverSocket.Stop();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
