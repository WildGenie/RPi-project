using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Linq;

namespace iContrAll.TcpServer
{
    public enum ClientState { LoginPhase, LoginOK }

    class ServiceHandler
    {
        private const int bufferSize = 32768;
        
        private TcpClient tcpClient;
        private ClientState clientState;

        public EndPoint Endpoint { get { return tcpClient.Client.RemoteEndPoint; } }

        public ServiceHandler(TcpClient client)
        {
            this.tcpClient = client;
            clientState = ClientState.LoginPhase;

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
                            Console.WriteLine("NumberOfBytesRead: {0}", numberOfBytesRead);
                        }
                        catch(ArgumentOutOfRangeException)
                        {
                            Console.WriteLine("The size of the message has exceeded the maximum size allowed.");
                            continue;
                        }

                        if (numberOfBytesRead <= 0) break;

                        Console.WriteLine("Message (length={1}) received from: {0} at {2}", tcpClient.Client.RemoteEndPoint.ToString(), numberOfBytesRead, DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture));

                        byte[] readBytes = readBuffer.Take(numberOfBytesRead).ToArray();

                        foreach (var message in ProcessBuffer(readBytes))
                        {
                            var result = ProcessMessage(message);
                            if (result == null || result.Length <= 0) continue;
                            // else reply!

                            // TODO: delete next line, it's just for debugging
                            Console.WriteLine("Response message: " + Encoding.UTF8.GetString(result));
                            // Reply to request
                            // Jójez, ez a cél, hogy ne várjuk meg a választ, gyorsabb legyen, bár nem számít nagyon.
                            await clientStream.WriteAsync(result, 0, result.Length);
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

        

        //byte[] trailingBuffer;
        private List<Message> ProcessBuffer(byte[] readBuffer)
        {
            var returnList = new List<Message>();
            
            // felfűzzük az elejére a maradékot
            byte[] completeBuffer;
            //if (trailingBuffer.Length > 0)
            //{
            //    completeBuffer = new byte[trailingBuffer.Length + readBuffer.Length];
            //    Array.Copy(trailingBuffer, completeBuffer, trailingBuffer.Length);
            //    Array.Copy(readBuffer, 0, completeBuffer, trailingBuffer.Length, readBuffer.Length);
            //}
            //else 
            completeBuffer = readBuffer;

            while (completeBuffer.Length > 0)
            {
                if (completeBuffer.Length<4) break;

                byte[] messageTypeArray = new byte[4];
                Array.Copy(completeBuffer, messageTypeArray, 4);

                int messageType = BitConverter.ToInt32(messageTypeArray, 0);

                if (clientState == ClientState.LoginPhase
                    && messageType != (byte)MessageType.LoginRequest)
                {
                    return returnList;
                }

                bool exists = false;

                // TODO: extract, do it only once!!
                foreach (var e in Enum.GetValues(typeof(MessageType)))
                {
                    if ((int)e == messageType)
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                {
                    
                    Console.WriteLine("Gyanus!");
                    break;
                    
                    //if (completeBuffer.Length > 4)
                    //{
                    //    var temp = completeBuffer;
                    //    temp.CopyTo(completeBuffer, 4);
                    //    completeBuffer = temp;
                    //}
                }

                if (completeBuffer.Length<8) break;

                byte[] messageLengthArray = new byte[4];
                Array.Copy(completeBuffer, 4, messageLengthArray, 0, 4);

                int messageLength = BitConverter.ToInt32(messageLengthArray, 0);

                // TODO: az összes ilyen esetkor (pl. kétszer feljebb) el kell tárolni a trailMessage-ben!
                if(completeBuffer.Length < 8 + messageLength) break;

                byte[] messageArray = new byte[messageLength];
                Array.Copy(completeBuffer, 8, messageArray, 0, messageLength);
                
                string message = Encoding.UTF8.GetString(messageArray);

                returnList.Add(new Message(messageType, messageLength, message));

                //Console.WriteLine(completeBuffer.Length + " - " + (8 + messageArray.Length));
                if (completeBuffer.Length >= 8 + messageArray.Length) // CONTINUE,
                {
                    byte[] tempBuf = new byte[completeBuffer.Length - (8 + messageArray.Length)];
                    Console.WriteLine(completeBuffer.Length + " - " + (8 + messageArray.Length) + " = tempBuf.Length: " + tempBuf.Length);
                    Array.Copy(completeBuffer, 8 + messageArray.Length, tempBuf, 0, completeBuffer.Length - (8 + messageArray.Length));
                    completeBuffer = tempBuf;
                }
            }

            return returnList;
        }

        private byte[] ProcessMessage(Message m)
        {
            byte messageType = (byte)m.Type;
            int messageLength = m.Length;
            string message = m.Content;

            Console.WriteLine("Message type: {0}\nMessage length: {1}\nMessage: {2}", messageType, messageLength, message);

            // TODO: kidolgozni!!!
            //Dictionary<MessageType, CreateAnswerDelegate> requestReplyMap = new Dictionary<MessageType, CreateAnswerDelegate>();
            //requestReplyMap.Add(MessageType.LoginRequest, CreateLoginResponse);

        //    delegate byte[] CreateAnswerDelegate(string message);

            switch (messageType)
            {
                case (byte)MessageType.LoginRequest:
                    return BuildMessage(15, CreateLoginResponse(message));
                case (byte)MessageType.QueryDeviceList:
                    return BuildMessage(7, CreateAnswerDeviceList());
                case (byte)MessageType.QueryDeviceDetails:
                    return BuildMessage(17, CreateAnswerDeviceDetails());
                case (byte)MessageType.AddDevice:
                    AddDevice(message);
                    break;
                case (byte)MessageType.DelDevice:
                    DelDevice(message);
                    break;
                case (byte)MessageType.eCmdGetPlaceList:
                    return BuildMessage(25, CreateAnswerPlaceList());
                case (byte)MessageType.eCmdAddPlace:
                case (byte)MessageType.eCmdRenamePlace:
                    AddPlace(message);
                    break;
                case (byte)MessageType.eCmdDelPlace:
                    DelPlace(message);
                    break;
                case (byte)MessageType.eCmdAddDeviceToPlace:
                    AddOrDelDeviceToOrFromPlace(true, message);
                    break;
                case (byte)MessageType.eCmdDelDeviceFromPlace:
                    AddOrDelDeviceToOrFromPlace(false, message);
                    break;
                case (byte)MessageType.eGetActionLists:
                    return BuildMessage(32, CreateAnswerActionList());
                case (byte)MessageType.eCmdAddActionList:
                    AddActionList(message);
                    break;
                case (byte)MessageType.eCmdDelActionList:
                    DelActionList(message);
                    break;
                case (byte)MessageType.eCmdAddActionToActionList:
                    AddOrDelActionToOrFromActionList(true, message);
                    break;
                case (byte)MessageType.eCmdDelActionFromActionList:
                    AddOrDelActionToOrFromActionList(false, message);
                    break;
                default:
                    break;
            }

            return null;
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
            string login = "";
            string password = "";

            bool loginOK = true;

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(message);

                // TODO:
                //      Do some check!
                //      LOGIN KEZELÉS!!!
                XmlNodeList elemList = doc.GetElementsByTagName("loginid");

                if (elemList.Count > 0)
                {
                    login = elemList[0].InnerXml;
                    if (login!=System.Configuration.ConfigurationManager.AppSettings["loginid"])
                    {
                        loginOK = false;
                    }
                }
                elemList = doc.GetElementsByTagName("password");
                if (elemList.Count > 0)
                {
                    password = elemList[0].InnerXml;
                }
            }
            catch(Exception)
            {
                Console.WriteLine("Error in Xml parsing.");
                return null;
            }

            

            LoginReasonType reason = LoginReasonType.Normal;

            byte[] response = new byte[8];

            response[0] = Convert.ToByte(loginOK);
            response[4] = (byte)reason;

            if (loginOK) clientState = ClientState.LoginOK;

            return response;
        }

        private byte[] CreateAnswerDeviceList()
        {
            Console.WriteLine("AnswerDeviceList called.");
            byte[] answer;
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
                                xw.WriteElementString("timer", channel.Timer);
                                xw.WriteElementString("voltage", channel.Voltage.ToString());
                                xw.WriteEndElement();
                                xw.WriteStartElement("actions");
                                foreach (var action in channel.Actions)
                                {
                                    xw.WriteStartElement("action");
                                    xw.WriteAttributeString("id", action.Id.ToString());
                                    xw.WriteAttributeString("name", action.Name.ToString());
                                    xw.WriteEndElement();
                                }
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
                answer = memStream.ToArray();
                memStream.Close();
                memStream.Dispose();
            }
            return answer;
        }

        private byte[] CreateAnswerDeviceDetails()
        {
            Console.WriteLine("AnswerDeviceDetails called.");
            byte[] answer;
            using (var dal = new DataAccesLayer())
            {
                IEnumerable<Device> deviceList = dal.GetDeviceList();
                //IEnumerable<Device> deviceList = DummyDb.GetDummyDevice();

                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Encoding = Encoding.UTF8;

                MemoryStream memStream = new MemoryStream();
                using (XmlWriter xw = XmlWriter.Create(memStream, settings))
                {
                    xw.WriteStartDocument();
                    xw.WriteStartElement("root");
                    foreach (var device in deviceList)
                    {
                        xw.WriteStartElement("dev");
                        xw.WriteAttributeString("id", device.Id);
                        xw.WriteAttributeString("ch", device.Channel.ToString());
                        xw.WriteAttributeString("name", device.Name);
                        xw.WriteEndElement();
                    }
                    xw.WriteEndElement();
                    xw.WriteEndDocument();
                    xw.Flush();
                    xw.Close();
                }
                answer = memStream.ToArray();
                memStream.Close();
                memStream.Dispose();
            }
            return answer;
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

            elemList = doc.GetElementsByTagName("timer");
            string elemTimer = "";

            if (elemList.Count > 0)
            {
                elemTimer = elemList[0].InnerXml;
            }

            int elemVoltage = 0;
            elemList = doc.GetElementsByTagName("voltage");
            if (elemList.Count > 0)
            {
                int.TryParse(elemList[0].InnerXml, out elemVoltage);
                

            }

            using (var dal = new DataAccesLayer())
            {
                dal.AddDevice(elemId, elemChannel, elemName, elemTimer, elemVoltage);
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

        private byte[] CreateAnswerPlaceList()
        {
            Console.WriteLine("AnswerPlaceList called.");
            byte[] answer;
            using (var dal = new DataAccesLayer())
            {
                IEnumerable<Place> placeList = dal.GetPlaceList();
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Encoding = Encoding.UTF8;

                MemoryStream memStream = new MemoryStream();
                using (XmlWriter xw = XmlWriter.Create(memStream, settings))
                {
                    xw.WriteStartDocument();
                    xw.WriteStartElement("root");
                    foreach (var p in placeList)
                    {
                        xw.WriteStartElement("room");
                        xw.WriteAttributeString("id", p.Id.ToString());
                        xw.WriteAttributeString("name", p.Name);
                        foreach (var d in p.DevicesInPlace)
                        {
                            xw.WriteStartElement("dev");
                            xw.WriteAttributeString("id", d.Id.ToString());
                            xw.WriteAttributeString("ch", d.Channel.ToString());
                            xw.WriteEndElement();
                        }

                        xw.WriteEndElement();
                    }
                    xw.WriteEndElement();
                    xw.WriteEndDocument();
                    xw.Flush();
                    xw.Close();
                }
                answer = memStream.ToArray();
                memStream.Close();
                memStream.Dispose();
            }
            return answer;
        }

        private void AddPlace(string message)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(message);

            XmlNodeList elemList = doc.GetElementsByTagName("id");
            string elemId = "";

            if (elemList.Count > 0)
            {
                elemId = elemList[0].InnerXml;
            }

            elemList = doc.GetElementsByTagName("name");
            string elemName = "";

            if (elemList.Count > 0)
            {
                elemName = elemList[0].InnerXml;
            }

            using (var dal = new DataAccesLayer())
            {
                dal.AddPlace(elemId, elemName);
            }

        }

        private void DelPlace(string message)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(message);

            XmlNodeList elemList = doc.GetElementsByTagName("id");
            string elemId = "";

            if (elemList.Count > 0)
            {
                elemId = elemList[0].InnerXml;
            }

            using (var dal = new DataAccesLayer())
            {
                dal.DelPlace(elemId);
            }
        }

        private void AddOrDelDeviceToOrFromPlace(bool p,string message)
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
            elemList = doc.GetElementsByTagName("ch");
            if (elemList.Count > 0)
            {
                int.TryParse(elemList[0].InnerXml, out elemChannel);
            }

            elemList = doc.GetElementsByTagName("room");
            Guid elemPlace = Guid.Empty;

            if (elemList.Count > 0)
            {
                elemPlace = new Guid(elemList[0].InnerXml);
            }

            using (var dal = new DataAccesLayer())
            {
                if (p)
                    dal.AddDeviceToPlace(elemId, elemChannel, elemPlace);
                else
                    dal.DelDeviceFromPlace(elemId, elemChannel, elemPlace);
            }
        }

        private byte[] CreateAnswerActionList()
        {
            Console.WriteLine("AnswerActionList called.");
            byte[] answer;
            using (var dal = new DataAccesLayer())
            {
                IEnumerable<ActionList> actionLists = dal.GetActionLists();

                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Encoding = Encoding.UTF8;

                MemoryStream memStream = new MemoryStream();
                using (XmlWriter xw = XmlWriter.Create(memStream, settings))
                {
                    xw.WriteStartDocument();
                    xw.WriteStartElement("root");
                    foreach (var actionList in actionLists)
                    {
                        xw.WriteStartElement("actionlist");
                        xw.WriteAttributeString("id", actionList.Id.ToString());
                        xw.WriteAttributeString("name", actionList.Name.ToString());
                        foreach (var action in actionList.Actions)
                        {
                            xw.WriteStartElement("action");
                            xw.WriteAttributeString("order", action.Order.ToString());
                            xw.WriteAttributeString("actionid", action.ActionTypeName);
                            xw.WriteAttributeString("to", action.DeviceId);
                            xw.WriteEndElement();
                        }

                        xw.WriteEndElement();
                    }
                    xw.WriteEndElement();
                    xw.WriteEndDocument();
                    xw.Flush();
                    xw.Close();
                }
                answer = memStream.ToArray();
                memStream.Close();
                memStream.Dispose();
            }
            return answer;
        }

        private void AddActionList(string message)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(message);

            XmlNodeList elemList = doc.GetElementsByTagName("id");
            string elemId = "";

            if (elemList.Count > 0)
            {
                elemId = elemList[0].InnerXml;
            }

            elemList = doc.GetElementsByTagName("name");
            string elemName = "";

            if (elemList.Count > 0)
            {
                elemName = elemList[0].InnerXml;
            }

            using (var dal = new DataAccesLayer())
            {
                dal.AddActionList(elemId, elemName);
            }
        }

        private void DelActionList(string message)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(message);

            XmlNodeList elemList = doc.GetElementsByTagName("id");
            string elemId = "";

            if (elemList.Count > 0)
            {
                elemId = elemList[0].InnerXml;
            }

            using (var dal = new DataAccesLayer())
            {
                dal.DelActionList(elemId);
            }
        }

        private void AddOrDelActionToOrFromActionList(bool p, string message)
        {
            XElement xelement = XElement.Parse(message);
            IEnumerable<XElement> actionEntries = xelement.Elements();

            var actionsToAddOrDel = from action in actionEntries
                                    select new
                                    {
                                        ActionListId = new Guid(action.Element("actionlistid").Value),
                                        ActionId = int.Parse(action.Element("actionid").Value),
                                        Order = int.Parse(action.Element("order").Value),
                                        DeviceId = action.Element("to").Value
                                    };

            using (var dal = new DataAccesLayer())
            {
                foreach (var a in actionsToAddOrDel)
                {
                    if (p)
                        dal.AddActionToActionList(a.ActionListId, a.ActionId, a.Order, a.DeviceId);
                    else
                        dal.DelActionFromActionList(a.ActionListId, a.ActionId, a.Order, a.DeviceId);
                }
            }
        }
    }
}
