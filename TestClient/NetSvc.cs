using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ILNet.Tools;
using ILNet.Mgr;
using Proto;
using ILNet.Chat;

public class NetSvc : ISingleton<NetSvc>
{




    private static readonly string obj = "lock";
    private static IClientSession<ClientSession,GameMsg> client;

    public static AckMgr<ClientSession,NetAckMsg> ackMgr;

    private Queue<GameMsg> msgQue = new Queue<GameMsg>();

    public void InitSvc()
    {


        client = new IClientSession<ClientSession, GameMsg>();
        client.StartCreate(SrvCfg.srvIP, SrvCfg.srvPort);
        

        client.SetLog(true, (string msg, int lv) =>
        {
            switch (lv)
            {
                case 0:
                    msg = "Log:" + msg;
                    NetLogger.LogMsg("消息 1：" + msg);
                    break;
                case 1:
                    msg = "Warn:" + msg;
                    NetLogger.LogMsg("消息 2" + msg, LogLevel.Warn);
                    break;
                case 2:
                    msg = "Error:" + msg;
                    NetLogger.LogMsg("消息 3：" + msg, LogLevel.Error);
                    break;
                case 3:
                    msg = "Info:" + msg;
                    NetLogger.LogMsg(msg, LogLevel.Info);
                    break;
            }
        });   
        NetLogger.LogMsg("Init NetSvc...");
    }

    #region Msg
    public void SendMsg(GameMsg msg)
    {
        if (client.Client!= null)
        {
            client.Client.SendMsg(msg);
        }
        else
        {
            NetLogger.LogMsg("服务器未连接");
            InitSvc();
        }
    }

    public void AddNetPkg(GameMsg msg)
    {
        lock (obj)
        {
            msgQue.Enqueue(msg);
        }
    }

    public void Update()
    {
        if (msgQue.Count > 0)
        {
            lock (obj)
            {
                GameMsg msg = msgQue.Dequeue();
                ProcessMsg(msg);
            }
        }
    }

    private void ProcessMsg(GameMsg msg)
    {
        NetLogger.LogMsg("处理数据:" + msg.chatMsg);
        if (msg.err != (int)ERR.None)
        {
            switch ((ERR)msg.err)
            {
                case ERR.None:
                    NetLogger.LogMsg("err   111");
                    break;
            }
            return;
        }
        switch ((CMD)msg.cmd)
        {
            case CMD.HelloWorld:
                //接收到心跳包 刷新
                if (ackMgr != null)
                    ackMgr.UpdateOneHeat(client.Client);
                break;
            case CMD.Chat:
                NetLogger.LogMsg($"接收到聊天信息{msg.chatMsg}");
                break;

        }
    }

    #endregion

    #region Ack

    public void StartSend()
    {
        {

           ackMgr = new AckMgr<ClientSession,NetAckMsg>().InitTimerEvent(send =>
            {
                client.Client.SendMsg(new GameMsg { cmd = (int)CMD.HelloWorld });
            }, obj =>
            {
                NetLogger.LogMsg("心跳包超时准备断开连接");
                if (obj != null)
                {
                   NetLogger.LogMsg(ackMgr.ConnectDic[obj].Lostcount.ToString());
                    obj.Clear();
                }

            }).StartTimer();
            ackMgr.AddConnectDic(client.Client, null,5, 5);
        };

    }


    #endregion


}

