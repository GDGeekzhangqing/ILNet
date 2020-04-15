using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ILNet.Mgr;
using ILNet.Chat;
using ILNet.Tools;
using Proltocol;

namespace TestServer
{
    class ServerSession:NetSession<GameMsg>
    {
        protected override void OnConnected()
        {
            NetLogger.LogMsg("Client OnLine.");
            SendMsg(new GameMsg
            {
                text = "Welcome to connect!"
            });
        }

        protected override void OnReciveMsg(GameMsg msg)
        {
            NetLogger.LogMsg("Client Request:" + msg);
            SendMsg(new GameMsg
            {
                text = msg.text
            });
        }

        protected override void OnDisConnected()
        {
           NetLogger.LogMsg("Client OffLine.");
        }

    }
}
