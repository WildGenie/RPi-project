using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iContrAll.TcpServer
{
    static class DummyDb
    {
        public static IEnumerable<Device> GetDummyDevice()
        {
            List<Device> deviceList = new List<Device>();

            deviceList.Add(new Device() { Id = "WC10010001", Name = "TesztEszkoz", Channel = 2 });

            return deviceList;
        }
    }
}
