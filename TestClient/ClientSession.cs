using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ILNet.Mgr;
using ILNet.Tools;
using Proltocol;


public class ClientSession :NetSession<GameMsg>
{
    protected override void OnConnected()
    {
        NetLogger.LogMsg("Connect Server Succ.");
    }

    protected override void OnReciveMsg(GameMsg msg)
    {
       NetLogger.LogMsg("Server Response:" + msg.text);
    }

    protected override void OnDisConnected()
    {
        NetLogger.LogMsg("Server Shutdown.");
    }
}

