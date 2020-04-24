using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ILNet.Mgr;
using ILNet.Tools;
using Proto;


public class ClientSession :ISession<GameMsg>
{
 
    protected override void OnConnected()
    {
        NetLogger.LogMsg("连接服务器成功.");
    }

    protected override void OnReciveMsg(GameMsg msg)
    {
       NetLogger.LogMsg("服务端回复消息:" + msg.chatMsg);
    }

    protected override void OnDisConnected()
    {
        base.OnDisConnected();
        NetLogger.LogMsg("本机下线.");
    }
}

