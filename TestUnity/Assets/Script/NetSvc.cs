/****************************************************
    文件：GameStart.cs
	作者：GDGeek^掌情
    邮箱: 1286358939@qq.com
    日期：#CreateTime#
	功能：Nothing
*****************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ILNet.Net;
using ILNet.Tools;
using Proto;

public class NetSvc : MonoBehaviour
{
    public static NetSvc Instance = null;

    private static readonly string obj = "lock";

    public IClientSession<ClientSession, GameMsg> session = new IClientSession<ClientSession, GameMsg>();

    public AckMgr<ClientSession, AckMsg> ackMgr;

    private Queue<GameMsg> msgQue = new Queue<GameMsg>();


    public void InitSvc()
    {
        Instance = this;

        session.StartCreate(SrvCfg.srvIP, SrvCfg.srvPort);
        SendAckToServer();

        //接收事件
        session.Client.OnReciveMsgEvent += AddNetPkg;


    }

    private void Update()
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



    private void OnDestroy()
    {
        ackMgr.Dispose();
        session.Client.Clear();
        Debug.Log("清理连接");
    }


    public void SendMsg(GameMsg msg)
    {
        if (session.Client != null)
        {
            session.Client.SendMsg(msg);
            Debug.Log("发送数据");
        }
        else
        {
            Debug.Log("服务器未连接"); //这里可以写断线重连的逻辑
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



    private void ProcessMsg(GameMsg msg)
    {
        Debug.Log("处理数据");
        if (msg.err != (int)ERR.None)
        {
            switch ((ERR)msg.err)
            {
                case ERR.None:
                    Debug.Log("没有错误码");
                    break;
            }
            return;
        }
        switch ((CMD)msg.cmd)
        {
            case CMD.HeartBeat:
                //接收到心跳包 刷新
                if (ackMgr != null)
                    ackMgr.UpdateOneHeat(session.Client);
                break;
            case CMD.RspLogin:
                Debug.Log($"接收到服务端消息：{msg.Chatdata}");
                break;
        }
    }


    public void SendAckToServer()
    {

        session.Client.OnStartConnectEvent += () =>
        {
            ackMgr = new AckMgr<ClientSession, AckMsg>().InitTimerEvent(send =>
             {
                 session.Client.SendMsg(new GameMsg { cmd = (int)CMD.HeartBeat });
                 Debug.Log("8888");
             },
             obj =>
             {
                 Debug.Log("心跳包超时准备断开连接...");
                 if (obj != null)
                 {
                     Debug.Log($"心跳连接数：{ackMgr.ConnectDic[obj].Lostcount}");
                     obj.Clear();
                 }

             }).StartTimer();

            ackMgr.AddConnectDic(session.Client, 5, 5);
        };

    }


    #region Test
    public void ClickSendBtn()
    {
        GameMsg msg = new GameMsg
        {
            cmd = (int)CMD.ReqLogin,
            Chatdata = new SendChat
            {
                chat = "Hello world，Server！",
                Islocal = 66,
            }
        };

        SendMsg(msg);

        Debug.Log("netSvc is:" + Instance);
    }


    #endregion

}