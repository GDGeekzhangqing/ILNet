using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ILNet.Mgr;
using ILNet.Net;
using ILNet.Tools;
using Proto;


public class ServerSession : ISession<GameMsg>
{
    public int SessionID = 0;
    public string PlayerName;
    public byte[] HeadData;

    protected override void OnStartConnected()
    {
        NetLogger.LogMsg($"ID:{SessionID}已下线");
    }


    protected override void OnReciveMsg(GameMsg data)
    {
        NetSvc.Instance.AddMsgQue(this, data);
        NetLogger.LogMsg($"接收到{SessionID}的请求:{data.ToString()}");
    }

    protected override void OnDisConnected()
    {
        SessionID = NetSvc.Instance.GetSessionID();
    }
}

