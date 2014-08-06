using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace iContrAll.TcpServer
{
    class Server
    {
        private TcpListener tcpListener;
        private Thread listenThread;
        private int port;

        private List<ClientClass> clientList = new List<ClientClass>();

        private object syncObject = new object();

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
                ////blocks until a client has connected to the server
                //TcpClient client = this.tcpListener.AcceptTcpClient();
                
                //blocks until a client has connected to the server
                ClientClass c = new ClientClass(this.tcpListener.AcceptTcpClient());

                lock(syncObject)
                {
                    clientList.Add(c);
                    
                }

                //create a thread to handle communication 
                //with connected client
                ////Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
                //Thread clientThread = new Thread(()=>HandleClientComm(client, i++));
                ////clientThread.Start(client);
                //clientThread.Start();
            }
        }

        private void HandleClientComm(TcpClient tcpClient, int i)
        {
            //TcpClient tcpClient = (TcpClient)client;
            NetworkStream clientStream = tcpClient.GetStream();

            byte[] rawMessage = new byte[4096];
            int bytesRead;

            if (clientStream.CanRead)
            {
                byte[] myReadBuffer = new byte[1024];
                StringBuilder myCompleteMessage = new StringBuilder();
                int numberOfBytesRead = 0;

                // Incoming message may be larger than the buffer size. 
                do
                {
                    numberOfBytesRead = clientStream.Read(myReadBuffer, 0, myReadBuffer.Length);
                    Console.WriteLine(i+" "+numberOfBytesRead);
                    
                    myCompleteMessage.AppendFormat("{0}", Encoding.ASCII.GetString(myReadBuffer, 0, numberOfBytesRead));

                    if (numberOfBytesRead > 4) break;
                }
                while (clientStream.DataAvailable);

                // Print out the received message to the console.
                Console.WriteLine("You received the following message : " +
                                             myCompleteMessage);
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
    }
}
