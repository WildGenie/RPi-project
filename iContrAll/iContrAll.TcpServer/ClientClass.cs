using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace iContrAll.TcpServer
{
    public enum ClientState { LoginPhase, LoginOK }
    class ClientClass
    {
        TcpClient tcpClient;
        public EndPoint Endpoint { get { return tcpClient.Client.RemoteEndPoint; } }

        public ClientClass(TcpClient client)
        {
            this.tcpClient = client;
        }

        public bool JoinClient() { return false; }

        public void SendMessage(int msgNumber, string message)
        {
            byte[] msgNbrArray = new byte[4];
            Array.Copy(BitConverter.GetBytes(msgNumber), msgNbrArray, msgNbrArray.Length);
            
            byte[] messageArray = Encoding.UTF8.GetBytes(message);
            byte[] lengthArray = new byte[4];
            Array.Copy(BitConverter.GetBytes(messageArray.Length), lengthArray, lengthArray.Length);
            byte[] answer = new byte[4 + 4 + messageArray.Length];

            System.Buffer.BlockCopy(msgNbrArray, 0, answer, 0, msgNbrArray.Length);
            System.Buffer.BlockCopy(lengthArray, 0, answer, msgNbrArray.Length, lengthArray.Length);
            System.Buffer.BlockCopy(messageArray, 0, answer, msgNbrArray.Length + lengthArray.Length, messageArray.Length);
        }
    }
}
