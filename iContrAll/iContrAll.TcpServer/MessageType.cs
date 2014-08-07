using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iContrAll.TcpServer
{
    public enum MessageType
    {
        IdentityMsg = -1,
        DebugMsg = 0,
        RadioMsg = 1,
        QueryIPSettings = 2,
        AnswerIPSettings = 3,
        QueryServerSettings = 4,
        AnswerServerSettings = 5,
        QueryDeviceList = 6,
        AnswerDeviceList= 7,
        SetDeviceList = 8,
        LoginRequest = 14,
        AnswerToLoginRequest = 15,
        QueryDeviceDetails = 16,
        AnswerDeviceDetails = 17,
        QueryMessageHistory = 18,
        AddDevice = 21,
        DelDevice = 22,
        CmdSetPassword = 23,
        eCmdGetRoomList = 24,
        eRoomList = 25,
        eCmdAddRoom = 26,
        eCmdDelRoom = 27,
        eCmdAddDeviceToRoom = 28,
        eCmdDelDeviceFromRoom = 29,
        eCmdRenameRoom = 30,
        eGetActionLists = 31,
        eCmdActionLists = 32,
        eCmdAddActionList = 33,
        eCmdAddActionToActionList = 34,
        eCmdDelActionList = 35,
        eCmdDelActionFromActionList = 36,
        eCmdExecActionList = 37
    }
}
