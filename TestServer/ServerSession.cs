using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ILNet.Mgr;
using ILNet.Chat;
using ILNet.Tools;
using Proto;


public class ServerSession :ISession<GameMsg>
{

    public int sessionID = 0;

    public string PlayerName;



    protected override void OnConnected()
    {
        NetLogger.LogMsg($"{sessionID}客户端已连接.");
       
    }

    protected override void OnReciveMsg(GameMsg msg)
    {
        NetLogger.LogMsg($"{sessionID}客户端回复:" + msg.chatMsg);
        NetSvc.Instance.AddMsgQue(this,msg);
      
    }

    protected override void OnDisConnected()
    {
        sessionID = NetSvc.Instance.GetSessionID();
        NetLogger.LogMsg($"{sessionID}客户端离线.");
    }

}
