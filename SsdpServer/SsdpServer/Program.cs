using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SsdpServer
{
    class Program
    {
        static void Main(string[] args)
        {
            string identifier = args.Length > 0 ? args[0] : "urn:schemas-upnp-org:device:RlanDevice:1";

            var listener = new RequestListener(identifier);
            listener.Start();
        }
    }
}
