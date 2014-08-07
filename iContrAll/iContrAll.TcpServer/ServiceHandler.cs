using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Xml;

namespace iContrAll.TcpServer
{
    public enum ClientState { LoginPhase, LoginOK }

    class ProcessMessageResult
    {
        public bool Success { get; set; }
        public byte[] Answer { get; set; }
    }

    class ServiceHandler
    {
        private const int bufferSize = 16384;
        
        private TcpClient tcpClient;
        
        public EndPoint Endpoint { get { return tcpClient.Client.RemoteEndPoint; } }

        public ServiceHandler(TcpClient client)
        {
            this.tcpClient = client;

            Thread commThread = new Thread(HandleMessages);
            commThread.Start();
        }

        private async void HandleMessages()
        {
            NetworkStream clientStream = null;
            var readBuffer = new byte[bufferSize];
            int numberOfBytesRead = 0;

            try
            {
                clientStream = tcpClient.GetStream();

                if (clientStream.CanRead)
                {
                    while (true)
                    {
                        try
                        {
                            numberOfBytesRead = await clientStream.ReadAsync(readBuffer, 0, bufferSize);
                        }
                        catch(ArgumentOutOfRangeException)
                        {
                            Console.WriteLine("The size of the message has exceeded the maximum size allowed.");
                            continue;
                        }

                        if (numberOfBytesRead <= 0) break;

                        var result = ProcessMessage(readBuffer);

                        // Reply to request
                        if (result.Length > 0)
                        {
                            // Jójez, ez a cél, hogy ne várjuk meg a választ, gyorsabb legyen, bár nem számít nagyon.
                            clientStream.WriteAsync(result, 0, result.Length);
                        }
                    }
                }
            }
            finally
            {
                clientStream.Close();
                tcpClient.Close();
            }
        }

        private byte[] ProcessMessage(byte[] readBuffer)
        {
            byte[] messageTypeArray = new byte[4];
            Array.Copy(readBuffer, messageTypeArray, 4);

            int messageType = BitConverter.ToInt32(messageTypeArray, 0);

            byte[] messageLengthArray = new byte[4];
            Array.Copy(readBuffer, 4, messageLengthArray, 0, 4);

            int messageLength = BitConverter.ToInt32(messageLengthArray, 0);

            byte[] messageArray = new byte[messageLength];
            Array.Copy(readBuffer, 8, messageArray, 0, messageLength);

            string message = Encoding.UTF8.GetString(messageArray);

            // akkor kell bufferelni a következőre
            if (readBuffer.Length > 8 + messageLength)
            {
                Console.WriteLine("Houston!");
            }

            Console.WriteLine("Message type: {0}\nMessage length: {1}\nMessage: {2}", messageType, messageLength, message);

            switch (messageType)
            {
                case (byte)MessageType.LoginRequest:
                    return BuildMessage(15, CreateLoginResponse(message));
                case (byte)MessageType.QueryDeviceList:
                    return BuildMessage(7, CreateAnswerDeviceList());
                case (byte)MessageType.AddDevice:
                    AddDevice(message);
                    break;
                case (byte)MessageType.DelDevice:
                    DelDevice(message);
                    break;
                default:
                    break;

            }

            // return null;
            return new byte[0];
        }

        private byte[] BuildMessage(int msgNumber, byte[] message)
        {
            byte[] msgNbrArray = new byte[4];
            Array.Copy(BitConverter.GetBytes(msgNumber), msgNbrArray, msgNbrArray.Length);
            
            byte[] lengthArray = new byte[4];
            Array.Copy(BitConverter.GetBytes(message.Length), lengthArray, lengthArray.Length);
            
            byte[] answer = new byte[4 + 4 + message.Length];

            System.Buffer.BlockCopy(msgNbrArray, 0, answer, 0, msgNbrArray.Length);
            System.Buffer.BlockCopy(lengthArray, 0, answer, msgNbrArray.Length, lengthArray.Length);
            System.Buffer.BlockCopy(message, 0, answer, msgNbrArray.Length + lengthArray.Length, message.Length);

            return answer;
        }

        enum LoginReasonType { Normal = 0, ServiceExpired = 1 };

        private byte[] CreateLoginResponse(string message)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(message);

            string login = "";
            string password = "";

            // TODO:
            //      Do some check!
            //      LOGIN KEZELÉS!!!
            XmlNodeList elemList = doc.GetElementsByTagName("loginid");
            
            if (elemList.Count > 0)
            {
                login = elemList[0].InnerXml;
            }
            elemList = doc.GetElementsByTagName("password");
            if (elemList.Count > 0)
            {
                password = elemList[0].InnerXml;
            }

            bool loginOK = true;

            LoginReasonType reason = LoginReasonType.Normal;

            byte[] response = new byte[8];

            response[0] = Convert.ToByte(loginOK);
            response[4] = (byte)reason;

            return response;
        }

        private byte[] CreateAnswerDeviceList()
        {
            Console.WriteLine("AnswerDeviceList called.");
            using (var dal = new DataAccesLayer())
            {
                IEnumerable<Device> deviceList = dal.GetDeviceList();
                //IEnumerable<Device> deviceList = DummyDb.GetDummyDevice();

                var devicesById = from device in deviceList
                                  group device by device.Id into newGroup
                                  orderby newGroup.Key
                                  select newGroup;

                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Encoding = Encoding.UTF8;

                MemoryStream memStream = new MemoryStream();
                using (XmlWriter xw = XmlWriter.Create(memStream, settings))
                {
                    xw.WriteStartDocument();
                    xw.WriteStartElement("root");
                    foreach (var device in devicesById)
                    {
                        xw.WriteStartElement("device");

                            // TODO: 
                            //      automatikus típusdetekció, 
                            //      attribútumok lehetőleg automatikus feldolgozása (attribútumra foreach)
                            //      NEM IS KELL, MERT MINDEGYIKNEK UGYANAZOK AZ ATTRIBÚTUMAI!!! 

                            xw.WriteElementString("id", device.Key);
                            xw.WriteElementString("ping", "y");
                            xw.WriteElementString("mirror", "n");
                            xw.WriteElementString("version", "");
                            xw.WriteElementString("link", "y");
                            xw.WriteStartElement("channels");
                            foreach (var channel in device)
                            {
                                xw.WriteStartElement("ch");
                                xw.WriteElementString("id", channel.Channel.ToString());
                                xw.WriteElementString("name", channel.Name);
                                xw.WriteStartElement("attr");
                                xw.WriteElementString("timer", "000500");
                                xw.WriteElementString("voltage", "240");
                                xw.WriteEndElement();
                                xw.WriteStartElement("actions");
                                xw.WriteEndElement();
                                xw.WriteEndElement();
                            }

                            xw.WriteEndElement();
                        xw.WriteEndElement();
                    }
                    xw.WriteEndElement();
                    xw.WriteEndDocument();
                    xw.Flush();
                    xw.Close();
                }
                byte[] answer = memStream.ToArray();
                memStream.Close();
                memStream.Dispose();

                return answer;
            }
        }
        
        private void AddDevice(string message)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(message);
            
            XmlNodeList elemList = doc.GetElementsByTagName("id");
            string elemId="";

            if (elemList.Count>0)
            {
                elemId = elemList[0].InnerXml;
            }
            
            int elemChannel=0;
            elemList = doc.GetElementsByTagName("channel");
            if (elemList.Count > 0)
            {
                int.TryParse(elemList[0].InnerXml, out elemChannel);
            }

            elemList = doc.GetElementsByTagName("name");
            string elemName="";

            if (elemList.Count > 0)
            {
                elemName = elemList[0].InnerXml;
            }

            using (var dal = new DataAccesLayer())
            {
                dal.AddDevice(elemId, elemChannel, elemName);
            }

        }

        private void DelDevice(string message)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(message);

            XmlNodeList elemList = doc.GetElementsByTagName("id");
            string elemId = "";

            if (elemList.Count > 0)
            {
                elemId = elemList[0].InnerXml;
            }

            int elemChannel = 0;
            elemList = doc.GetElementsByTagName("channel");
            if (elemList.Count > 0)
            {
                int.TryParse(elemList[0].InnerXml, out elemChannel);
            }

            elemList = doc.GetElementsByTagName("name");
            string elemName = "";

            if (elemList.Count > 0)
            {
                elemName = elemList[0].InnerXml;
            }

            using (var dal = new DataAccesLayer())
            {
                dal.DelDevice(elemId, elemChannel);
            }
        }
    }
}
