using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ILNet.Chat;
using ILNet.Mgr;
using ILNet.Tools;
using Proto;
public class MsgPack
{
    public ServerSession session;
    public GameMsg msg;

    public MsgPack(ServerSession session, GameMsg msg)
    {
        this.session = session;
        this.msg = msg;
    }
}

public class NetSvc : IServerSession<ServerSession, GameMsg>
{
    public static readonly string obj = "lock";
   

    private static NetSvc instance;
    public static NetSvc Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new NetSvc();
            }
            return instance;
        }
        set => instance = value;
    }

    private Queue<MsgPack> msgPackQue = new Queue<MsgPack>();

    IServerSession<ServerSession, GameMsg> server;
    public AckMgr<ServerSession, NetAckMsg> ackMgr;


    public void Init()
    {
        server = new IServerSession<ServerSession, GameMsg>();
        StartCreate(SrvCfg.srvIP, SrvCfg.srvPort);
        
        NetLogger.LogMsg("NetSvc Init ...");
    }

    #region Msg

    public void AddMsgQue(ServerSession session, GameMsg msg)
    {
        lock (obj)
        {
            msgPackQue.Enqueue(new MsgPack(session, msg));
        }
    }

    public void Update()
    {
        if (msgPackQue.Count > 0)
        {
            lock (obj)
            {
                MsgPack pack = msgPackQue.Dequeue();
                HandOutMsg(pack);
            }
        }
    }


    private void HandOutMsg(MsgPack pack)
    {
        switch ((CMD)pack.msg.cmd)
        {
            case CMD.HelloWorld:
                //更新心跳包
                NetLogger.LogMsg("更新心跳包");
                ackMgr.UpdateOneHeat(pack.session);
                pack.session.SendMsg(pack.msg);
                break;
            case CMD.Chat:
                NetLogger.LogMsg("聊天信息："+pack.msg.chatMsg);
                break;
        }
    }

    #endregion


    #region Cache

    public int sessionID = 0;

    public int GetSessionID()
    {
        if (sessionID == int.MaxValue)
        {
            sessionID = 0;
        }
        return sessionID += 1;
    }

    public bool IsUserOnLine(string name)
    {
        return sessionLst.Select(v => v.PlayerName).ToList().Contains(name);
    }



    #endregion


    #region ACK

 

    public override void StartCreate(string ip, int port)
    {
        base.StartCreate(ip, port);
        NetLogger.LogMsg("开始创建会话");

        ackMgr = new AckMgr<ServerSession, NetAckMsg>().InitTimerEvent(null, lost =>
          {
              if (lost != null && lost.skt != null && lost.skt.Connected)
              {
                  NetLogger.LogMsg($"{lost.sessionID}的心跳包超时，准备断开连接", LogLevel.Error);
                  lost.Clear();
              }
          }).StartTimer();

        //添加新的会话事件
        AddSessionEvent += v =>
        {
            ackMgr.AddConnectDic(v);
            NetLogger.LogMsg("添加新的会话事件");
        };

        //删除会话事件
        RemoveSessionEvent += v =>
        {
            ackMgr.RemoveConnectDic(v);
            NetLogger.LogMsg("删除会话事件");
        };

    }


    #endregion

}
