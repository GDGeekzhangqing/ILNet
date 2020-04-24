/****************************************************
    文件：GameApp.cs
	作者：GDGeek^掌情
    邮箱: 1286358939@qq.com
    日期：#CreateTime#
	功能：Nothing
*****************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ILNet.Tools;
using Proto;
using ILNet.Mgr;
using ILNet.Chat;

public class GameApp : MonoBehaviour
{

    #region 旧的

   // public NetSvc netSvc = null;



   /* public void Start()
    {
        netSvc = GetComponent<NetSvc>();
        netSvc.InitSvc();

        NetLogger.LogMsg("GameApp Start...");
    }*/



    /* private void Update()
     {
         if (netSvc != null && Input.GetKeyDown(KeyCode.Space))
         {
             netSvc.SendMsg(new GameMsg
             {
                 cmd = (int)CMD.Chat,
                 chatMsg = "Hello My Server!"
             });
         }
     }*/

    #endregion

    #region Test



    private static readonly string obj = "lock";

    public IClientSession<ClientSession, GameMsg> session = null;

    public AckMgr<ClientSession, NetAckMsg> ackMgr;

    private Queue<GameMsg> msgQue = new Queue<GameMsg>();


    public void Start()
    {     
        session = new IClientSession<ClientSession, GameMsg>();
        session.StartCreate(SrvCfg.srvIP, SrvCfg.srvPort);
        SendAckToServer();

        //接收事件
        session.Client.OnReciveMsgEvent += AddNetPkg;


        session.SetLog(true, (string msg, int lv) =>
        {
            switch (lv)
            {
                case 0:
                    msg = "Log:" + msg;
                    Debug.Log(msg);
                    break;
                case 1:
                    msg = "Warn:" + msg;
                    Debug.LogWarning(msg);
                    break;
                case 2:
                    msg = "Error:" + msg;
                    Debug.LogError(msg);
                    break;
                case 3:
                    msg = "Info:" + msg;
                    Debug.Log(msg);
                    break;
            }
        });


    }

    private void Update()
    {
        // SendAckToServer();
        if (msgQue.Count > 0)
        {
            lock (obj)
            {
                GameMsg msg = msgQue.Dequeue();
                ProcessMsg(msg);
            }
        }
    }

   

    private void OnDestroy()
    {
        ackMgr.Dispose();
        session.Client.Clear();
        NetLogger.LogMsg("清理连接");
    }


    public void SendMsg(GameMsg msg)
    {
        if (session.Client != null)
        {
            session.Client.SendMsg(msg);
            NetLogger.LogMsg("发送数据");
        }
        else
        {
            NetLogger.LogMsg("服务器未连接"); //这里可以写断线重连的逻辑
            //InitSvc();
        }
    }


    public void AddNetPkg(GameMsg msg)
    {
        lock (obj)
        {
            msgQue.Enqueue(msg);
        }
    }



    private void ProcessMsg(GameMsg msg)
    {
        Debug.Log("处理数据");
        if (msg.err != (int)ERR.None)
        {
            switch ((ERR)msg.err)
            {
                case ERR.None:
                    NetLogger.LogMsg("没有错误码");
                    break;
            }
            return;
        }
        switch ((CMD)msg.cmd)
        {
            case CMD.HelloWorld:
                //接收到心跳包 刷新
                if (ackMgr != null)
                    ackMgr.UpdateOneHeat(session.Client);
                break;
            case CMD.Chat:
                NetLogger.LogMsg($"接收到服务端消息：{msg.chatMsg}");
                break;
        }
    }




    public void SendAckToServer()
    {
        Debug.Log("客户端实例状态：" + session.Client);
        session.Client.OnConnectEvent += () =>
        {
            ackMgr = new AckMgr<ClientSession, NetAckMsg>().InitTimerEvent(send =>
            {
                session.Client.SendMsg(new GameMsg { cmd = (int)CMD.HelloWorld });
                NetLogger.LogMsg("8888");
            },
             obj =>
             {
                 NetLogger.LogMsg("心跳包超时准备断开连接...");
                 if (obj != null)
                 {
                     NetLogger.LogMsg($"心跳连接数：{ackMgr.ConnectDic[obj].Lostcount}");
                     obj.Clear();
                 }

             }).StartTimer();

            ackMgr.AddConnectDic(session.Client, null, 5, 5);
        };

        //接收事件
        session.Client.OnReciveMsgEvent += AddNetPkg;


    }


    #endregion

}