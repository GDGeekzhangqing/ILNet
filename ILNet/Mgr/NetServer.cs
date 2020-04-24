using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ILNet.Chat;
using ILNet.Lua;
using ILNet.Tools;

namespace ILNet.Mgr
{
    public class NetServer<T, K> where T : NetSession<K>, new() where K : NetMsg
    {

        private Socket skt = null;
        public T session = null;
        public int backLog = 10;
        List<T> sessionLst = new List<T>();

      

        public NetServer()
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
                skt.BeginAccept(new AsyncCallback(ClientConnectCB), skt);
                NetLogger.LogMsg("\nServer Start Success!\nWaiting for Connecting......", LogLevel.Info);
            }
            catch (Exception e)
            {
                NetLogger.LogMsg(e.Message, LogLevel.Error);
            }
        }

        void ClientConnectCB(IAsyncResult ar)
        {
            try
            {
                Socket clientSkt = skt.EndAccept(ar);
                T session = new T();
                sessionLst.Add(session);
                session.StartRcvData(clientSkt, () =>
                {
                    if (sessionLst.Contains(session))
                    {
                        sessionLst.Remove(session);
                    }
                });

                //开始为服务端添加上线和离线的监听事件
                session.OnClientOffline += ClientOfflineEvent;
                session.OnClientOnline += ClientOnlineEvent;

                //开启扫描离线线程
                Thread client = new Thread(new ThreadStart(ScanClientConnectCB));
                client.IsBackground = true;
                client.Start();
                NetLogger.LogMsg("开始扫描离线程序");

            }
            catch (Exception e)
            {
                NetLogger.LogMsg(e.Message, LogLevel.Error);
            }
            skt.BeginAccept(new AsyncCallback(ClientConnectCB), skt);
        }

        /// <summary>
        /// 开始扫描离线线程
        /// </summary>
        public void ScanClientConnectCB()
        {
            session.ScanOffline();
            NetLogger.LogMsg("开始扫描离线线程...");
        }

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


        #endregion


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

    }
}
