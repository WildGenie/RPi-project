using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using MySql.Data.MySqlClient;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using System.IO;

namespace iContrAll.TcpServer
{
    class Server
    {
        private TcpListener tcpListener;
        private Thread listenThread;
        private int port;

        //private List<ClientClass> clientList = new List<ClientClass>();

        //private object syncObject = new object();

        public Server(int port)
        {
            this.port = port;
            this.tcpListener = new TcpListener(IPAddress.Any, this.port);
            this.listenThread = new Thread(new ThreadStart(ListenForClients));
            this.listenThread.Start();
        }

        private void ListenForClients()
        {
            this.tcpListener.Start();
            Console.WriteLine("Server is listening on port {0}...", this.tcpListener.LocalEndpoint);

            int i = 0;

            while (true)
            {
                //blocks until a client has connected to the server
                TcpClient client = this.tcpListener.AcceptTcpClient();
                
                ////blocks until a client has connected to the server
                //ClientClass c = new ClientClass(this.tcpListener.AcceptTcpClient());

                //lock(syncObject)
                //{
                //    clientList.Add(c);
                    
                //}

                //create a thread to handle communication 
                //with connected client
                ////Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
                Thread clientThread = new Thread(()=>HandleClientComm(client, i++));
                ////clientThread.Start(client);
                clientThread.Start();
            }
        }

        struct ProcessMessageResult
        {
            public bool Success;
            public byte[] Answer;
        }

        private void HandleClientComm(TcpClient tcpClient, int i)
        {
            
            //TcpClient tcpClient = (TcpClient)client;
            NetworkStream clientStream = tcpClient.GetStream();

            //byte[] rawMessage = new byte[32768];
            //int bytesRead;

            if (clientStream.CanRead)
            {
                
                StringBuilder myCompleteMessage = new StringBuilder();
                int numberOfBytesRead = 0;

                // Incoming message may be larger than the buffer size. 
                while(true)
                {
                    byte[] myReadBuffer = new byte[32768];
                    // TODO:
                    //      try-catch block for too large messages!!!
                    numberOfBytesRead += clientStream.Read(myReadBuffer, 0, myReadBuffer.Length);

                    if (!tcpClient.Connected) Console.WriteLine("NINCS KONNEKTOLVA!");

                    Console.WriteLine(i+" "+numberOfBytesRead);
                    
                    myCompleteMessage.AppendFormat("{0}", Encoding.UTF8.GetString(myReadBuffer, 0, numberOfBytesRead));
                    
                    Console.WriteLine("You received the following message : " + myCompleteMessage);
                    //if (numberOfBytesRead > 4) break;

                    //byte[] answer = BuildMessage(15, new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 });
                    //byte[] answer = BuildMessage(7, AnswerDeviceList());
                    ProcessMessageResult result = ProcessMessage(myReadBuffer, numberOfBytesRead);
                    if (result.Answer.Length > 0)
                    {
                        clientStream.Write(result.Answer, 0, result.Answer.Length);
                        clientStream.Flush();
                        Console.WriteLine("Replied: {0}", Encoding.UTF8.GetString(result.Answer));
                    }
                    if (result.Success || numberOfBytesRead > 16384)
                    {
                        // csak akkor nullázzuk, ha sikerült feldolgozni.
                        // aztán mondjuk megszopjuk, ha nem
                        numberOfBytesRead = 0;
                        myCompleteMessage.Clear();
                    }
                }
                //while (clientStream.DataAvailable);

                // Print out the received message to the console.
                //Console.WriteLine("You received the following message : " + myCompleteMessage);


                //// FONTOS KÓD!!!
                //byte[] answer = ProcessMessage(myReadBuffer, numberOfBytesRead);
                ////byte[] answer = BuildMessage(7, AnswerDeviceList());
                //if (answer.Length > 0)
                //{
                //    clientStream.Write(answer, 0, answer.Length);
                //    clientStream.Flush();
                //    Console.WriteLine("Replied: {0}", Encoding.UTF8.GetString(answer));
                //}
            }
            else
            {
                Console.WriteLine("Sorry.  You cannot read from this NetworkStream.");
            }

            //while (true)
            //{
            //    bytesRead = 0;

            //    try
            //    {
            //        //blocks until a client sends a message
            //        bytesRead = clientStream.Read(rawMessage, 0, 4096);
            //    }
            //    catch
            //    {
            //        //a socket error has occured
            //        break;
            //    }

            //    Console.WriteLine(Encoding.UTF8.GetString(rawMessage));

            //    if (bytesRead == 0)
            //    {
            //        //the client has disconnected from the server
            //        break;
            //    }
            //    else
            //        if (bytesRead < 8) continue;

            //    byte[] messageTypeArray = new byte[4];
            //    Array.Copy(rawMessage, messageTypeArray, 4);

            //    int messageType = BitConverter.ToInt32(messageTypeArray, 0);

            //    if (messageType > 40) continue;

            //    byte[] messageLengthArray = new byte[4];
            //    Array.Copy(rawMessage, 4, messageLengthArray, 0, 4);

            //    int messageLength = BitConverter.ToInt32(messageLengthArray, 0);

            //    byte[] messageArray = new byte[messageLength];
            //    Array.Copy(rawMessage, 8, messageArray, 0, messageLength);

            //    string message = Encoding.UTF8.GetString(messageArray);

            //    Console.WriteLine("Message type: {0}\nMessage length: {1}\nMessage: {2}", messageType, messageLength, message);
                
            //    // Login request
            //    if (messageType == 14)
            //    {

            //        int replyMessageNumber = 15;
            //        byte[] msgNbrArray = BitConverter.GetBytes(replyMessageNumber);
            //        byte[] answerToLoginRequest = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0, 0 };

            //        byte[] length = BitConverter.GetBytes(answerToLoginRequest.Length);

            //        byte[] answer = new byte[msgNbrArray.Length + length.Length + answerToLoginRequest.Length];
            //        System.Buffer.BlockCopy(msgNbrArray, 0, answer, 0, msgNbrArray.Length);
            //        System.Buffer.BlockCopy(length, 0, answer, msgNbrArray.Length, length.Length);
            //        System.Buffer.BlockCopy(answerToLoginRequest, 0, answer, msgNbrArray.Length + length.Length, answerToLoginRequest.Length);
            //        clientStream.Write(answer, 0, answer.Length);
            //        clientStream.Flush();
            //    }

            //    ////message has successfully been received
            //    //Encoding encoder = Encoding.UTF8;
            //    //string stringMessage = encoder.GetString(rawMessage, 4, bytesRead - 4);

            //    //if (stringMessage.Contains("GET / HTTP/1.1"))
            //    //{
            //    //    byte[] sendBuf = encoder.GetBytes("HTTP/1.1 200 OK");
            //    //    clientStream.Write(sendBuf, 0, sendBuf.Length);
            //    //    clientStream.Flush();
            //    //}

            //    //Console.WriteLine(bytesRead);

            //    //Console.WriteLine(stringMessage + " "+stringMessage.Length);

            //    //byte[] answerToLoginRequest = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0, 0 };
            //    //clientStream.Write(answerToLoginRequest, 0, answerToLoginRequest.Length);
            //    //clientStream.Flush();

                
            //    //System.Diagnostics.Debug.WriteLine(encoder.GetString(message, 0, bytesRead));
            //}

            tcpClient.Close();
        }

        private ProcessMessageResult ProcessMessage(byte[] rawMessage, int numberOfBytesRead)
        {
            //if (numberOfBytesRead == 0)
            //{
            //    //the client has disconnected from the server
            //    break;
            //}
            //else
            //    if (numberOfBytesRead < 8) continue;

            

            byte[] messageTypeArray = new byte[4];
            Array.Copy(rawMessage, messageTypeArray, 4);

            int messageType = BitConverter.ToInt32(messageTypeArray, 0);

            //if (messageType > 40) continue;

            byte[] messageLengthArray = new byte[4];
            Array.Copy(rawMessage, 4, messageLengthArray, 0, 4);

            int messageLength = BitConverter.ToInt32(messageLengthArray, 0);

            byte[] messageArray = new byte[messageLength];
            Array.Copy(rawMessage, 8, messageArray, 0, messageLength);

            string message = Encoding.UTF8.GetString(messageArray);

            Console.WriteLine("Message type: {0}\nMessage length: {1}\nMessage: {2}", messageType, messageLength, message);

            switch (messageType)
            {
                case (byte)MessageType.LoginRequest:
                    if (CheckLoginDetails(message))
                    {
                        return new ProcessMessageResult { Success = true, Answer = BuildMessage(15, new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 }) };
                    }
                    break;
                case (byte)MessageType.QueryDeviceList:
                    return new ProcessMessageResult { Success = true, Answer = BuildMessage(7, AnswerDeviceList()) };
                case (byte)MessageType.AddDevice:
                    AddDevice(message);
                    return new ProcessMessageResult { Success = true, Answer = new byte[0] };
                    
                case (byte)MessageType.DelDevice:
                    DelDevice(message);
                    return new ProcessMessageResult { Success = true, Answer = new byte[0] };
                    
                default:
                    return new ProcessMessageResult { Success = false, Answer = new byte[0] };
                    
            }

            return new ProcessMessageResult { Success = false, Answer = new byte[0] };
        }

        public byte[] BuildMessage(int msgNumber, byte[] message)
        {
            byte[] msgNbrArray = new byte[4];
            Array.Copy(BitConverter.GetBytes(msgNumber), msgNbrArray, msgNbrArray.Length);

            //byte[] messageArray = Encoding.UTF8.GetBytes(message);
            byte[] lengthArray = new byte[4];
            Array.Copy(BitConverter.GetBytes(message.Length), lengthArray, lengthArray.Length);
            //Array.Copy(BitConverter.GetBytes(messageArray.Length), lengthArray, lengthArray.Length);
            //byte[] answer = new byte[4 + 4 + messageArray.Length];
            byte[] answer = new byte[4 + 4 + message.Length];

            System.Buffer.BlockCopy(msgNbrArray, 0, answer, 0, msgNbrArray.Length);
            System.Buffer.BlockCopy(lengthArray, 0, answer, msgNbrArray.Length, lengthArray.Length);
            //System.Buffer.BlockCopy(messageArray, 0, answer, msgNbrArray.Length + lengthArray.Length, messageArray.Length);
            System.Buffer.BlockCopy(message, 0, answer, msgNbrArray.Length + lengthArray.Length, message.Length);

            return answer;
        }

        private bool CheckLoginDetails(string message)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(message);

            // TODO:
            //      Do some check!
            //      LOGIN KEZELÉS!!!
            XmlNodeList elemList = doc.GetElementsByTagName("loginid");
            //Console.WriteLine(doc.GetElementById("loginid").Value);
            if (elemList.Count > 0)
            {
                Console.WriteLine(elemList[0].InnerXml);
            }
            elemList = doc.GetElementsByTagName("password");
            if (elemList.Count > 0)
            {
                Console.WriteLine(elemList[0].InnerXml);
            }

            //Console.WriteLine(doc.GetElementById("password").Value);

            return true;
        }

        private byte[] AnswerDeviceList()
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
