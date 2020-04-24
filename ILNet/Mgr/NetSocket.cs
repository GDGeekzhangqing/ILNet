using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ILNet.Chat;
using ILNet.Tools;

namespace ILNet.Mgr
{
    public class NetSocket<T,K> where T : NetSession<K>, new() where K : NetMsg, new() 
    {
        private Socket skt = null;
        public T session = null;
        public int backLog = 10;
        List<T> sessionLst = new List<T>();

        public event Action<T> AddSessionEvent;
        public event Action<T> RemoveSessionEvent;
        public event Action ConnectErrorEvent;


        


        public NetSocket()
        {
            //指定Socket属性
            skt = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        #region Server
        public void StartAsServer(string ip, int port)
        {
            try
            {
                skt.Bind(new IPEndPoint(IPAddress.Parse(ip), port));
                skt.Listen(backLog);
                //开始新一轮的接收连接
                skt.BeginAccept(new AsyncCallback(ClientConnectCB), skt);
                NetLogger.LogMsg("\n服务端启动成功!\n等待连接中......", LogLevel.Info);
            }
            catch (Exception e)
            {
                NetLogger.LogMsg("服务端异步启动错误:"+e.Message, LogLevel.Error);
            }
        }

        void ClientConnectCB(IAsyncResult ar)
        {
            try
            {
                Socket clientSkt = skt.EndAccept(ar);
                T session = new T();
                //存储会话
                sessionLst.Add(session);
                //开始监听
                session.StartRcvData(clientSkt, () =>
                {
                    if (sessionLst.Contains(session))
                    {
                        //添加移除客户端的监听
                        RemoveSessionEvent?.Invoke(session);
                        //从列表中移除
                        sessionLst.Remove(session);
                    }
                });

                //添加委托监听
                AddSessionEvent?.Invoke(session);    

            }
            catch (Exception e)
            {
                NetLogger.LogMsg("客户端 异步连接错误"+e.Message, LogLevel.Error);
            }
            skt.BeginAccept(new AsyncCallback(ClientConnectCB), skt);
        }
        #endregion

        #region Client
        public void StartAsClient(string ip, int port)
        {
            try
            {
                skt.BeginConnect(new IPEndPoint(IPAddress.Parse(ip), port), new AsyncCallback(ServerConnectCB), skt);
                NetLogger.LogMsg("\n客户端启动成功!\n准备连接服务端......", LogLevel.Info);
            }
            catch (Exception e)
            {
                NetLogger.LogMsg(e.Message, LogLevel.Error);
            }
        }

        void ServerConnectCB(IAsyncResult ar)
        {
            try
            {
                skt.EndConnect(ar);
                session = new T();
                session.StartRcvData(skt, null);



                // 开启心跳线程
                /* Thread server = new Thread(new ThreadStart(SendHeardBeatCB));
                 server.IsBackground = true;
                 server.Start();*/

            }
            catch (Exception e)
            {
                ConnectErrorEvent?.Invoke();
                NetLogger.LogMsg(e.Message, LogLevel.Error);
            }
        }

      
        #endregion

        public void Close()
        {
            if (skt != null)
            {
                skt.Close();
            }
        }



        public List<T> GetSesstionLst()
        {
            return sessionLst;
        }

        public void SetLog(bool log = true, Action<string, int> logCB = null)
        {
            if (log == false)
            {
                NetLogger.log = false;
            }
            if (logCB != null)
            {
                NetLogger.logCB = logCB;
            }
        }

        public virtual void AddSession(T obj) { }

        public virtual void RemoveSession(T obj) { }


     

        #region 将被弃用
        /// <summary>
        /// 客户端离线提示
        /// </summary>
        /// <param name="clientInfo"></param>
        public static void ClientOfflineEvent(ClientInfo clientInfo)
        {
            NetLogger.LogMsg(String.Format("客户端{0}离线，离线时间：\t{1}", clientInfo.ClientID, clientInfo.LastHeartbeatTime));
        }

        /// <summary>
        /// 客户端上线提示
        /// </summary>
        /// <param name="clientInfo"></param>
        public static void ClientOnlineEvent(ClientInfo clientInfo)
        {
            NetLogger.LogMsg(String.Format("客户端{0}上线，上线时间：\t{1}", clientInfo.ClientID, clientInfo.LastHeartbeatTime));
        }
        #endregion

    }
}
