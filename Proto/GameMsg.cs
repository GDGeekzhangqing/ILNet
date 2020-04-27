using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ILNet.Net;


namespace Proto
{
    [Serializable]
    public class GameMsg : NetMsg
    {
        public byte[] HeadData;//头像图片的数据

        public string PlayerName;

        public SendChat Chatdata;//聊天消息的数据

    }

    [Serializable]
    public enum ERR {
        None,
       
    }

    [Serializable]
    public enum CMD
    {
        None,
        HeartBeat,
        ReqLogin,
        RspLogin,
    }

    public class SrvCfg
    {
        public const string srvIP = "127.0.0.1";
        public const int srvPort = 17666;
    }

    [Serializable]
    public class SendChat
    {
        public string chat;
        public int Islocal;
    }

}
