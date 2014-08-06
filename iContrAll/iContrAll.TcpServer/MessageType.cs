using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iContrAll.TcpServer
{
    public enum MessageType
    {
        QueryDeviceList = 6,
        AnswerDeviceList= 7,
        LoginRequest = 14,
        AnswerToLoginRequest = 15,
        AddDevice = 21,
        DelDevice = 22,
    }
}
