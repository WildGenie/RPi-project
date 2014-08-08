using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;


namespace iContrAll.TcpServer
{
    class Server
    {
        private TcpListener tcpListener;
        private Thread listenThread;
        private int port;

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

            while (true)
            {
                //blocks until a client has connected to the server
                TcpClient client = this.tcpListener.AcceptTcpClient();
                Console.WriteLine("Client connected: {0}", client.Client.RemoteEndPoint);

                // TODO: start() a servicehandlernek
                ServiceHandler sh = new ServiceHandler(client);
            }
        }
    }
}
