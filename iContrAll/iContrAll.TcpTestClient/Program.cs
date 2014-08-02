using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace iContrAll.TcpTestClient
{
    class Program
    {
        static IPAddress address;
        static int port;
        static void Main(string[] args)
        {
            try
            {
                if (!(IPAddress.TryParse(args[0], out address) && int.TryParse(args[1], out port)))
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
            catch(Exception)
            {
                Console.Error.WriteLine("Arguments require:\n- Target IPv4 address (e.g. 192.168.1.100)\n- Port number");
                return;
            }

            string message;
            while ((message = Console.ReadLine()) != "")
            {
                Send(args[0], args[1], message);
            }
        }

        static void Send(string address, string port, string message)
        {
            TcpClient client = new TcpClient();

            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(address), 1122);

            client.Connect(serverEndPoint);

            NetworkStream clientStream = client.GetStream();

            ASCIIEncoding encoder = new ASCIIEncoding();
            byte[] buffer = encoder.GetBytes(message);

            clientStream.Write(buffer, 0, buffer.Length);
            clientStream.Flush();
        }
    }
}
