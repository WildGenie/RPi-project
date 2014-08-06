using System;
namespace iContrAll.TcpServer
{
    class Program
    {
        static void Main(string[] args)
        {
            new Server(1122);

            //foreach (MessageType mt in (MessageType[])Enum.GetValues(typeof(MessageType)))
            //{
            //    Console.WriteLine((byte)mt);
            //}

            //int msgNumber = 15;

            //byte[] messageArray = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0};

            //byte[] msgNbrArray = new byte[4];
            //Array.Copy(BitConverter.GetBytes(msgNumber), msgNbrArray, msgNbrArray.Length);

            //// byte[] messageArray = Encoding.UTF8.GetBytes(message);
            //byte[] lengthArray = new byte[4];
            //Array.Copy(BitConverter.GetBytes(messageArray.Length), lengthArray, lengthArray.Length);
            //byte[] answer = new byte[4 + 4 + messageArray.Length];

            //System.Buffer.BlockCopy(msgNbrArray, 0, answer, 0, msgNbrArray.Length);
            //System.Buffer.BlockCopy(lengthArray, 0, answer, msgNbrArray.Length, lengthArray.Length);
            //System.Buffer.BlockCopy(messageArray, 0, answer, msgNbrArray.Length + lengthArray.Length, messageArray.Length);
        }
    }
}
