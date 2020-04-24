using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ILNet.Mgr;
using ILNet.Chat;

namespace Proto
{
    [Serializable]
    public class GameMsg : NetMsg
    {
        public string playerName;

        public string chatMsg;

    }

    [Serializable]
    public enum ERR {
       None
    }

    [Serializable]
    public enum CMD
    {
        HelloWorld,
        Chat
    }

    public class SrvCfg
    {
        public const string srvIP = "127.0.0.1";
        public const int srvPort = 17666;
    }

}
