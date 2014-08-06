using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iContrAll.TcpServer
{
    class Device
    {
        private string id;
        public string Id
        {
            get { return id; }
            set { id = value; }
        }

        private int channel;
        public int Channel
        {
            get { return channel; }
            set { channel = value; }
        }

        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        
    }
}
