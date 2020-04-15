using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ILNet.Chat;

namespace Proltocol
{
    [Serializable]
    public class GameMsg:NetMsg
    {
        public string text;

    }
    public class IPCfg
    {
        public const string srvIP = "127.0.0.1";
        public const int srvPort = 17666;
    }
}
