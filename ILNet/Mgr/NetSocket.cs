using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ILNet.Chat;
using ILNet.Tools;

namespace ILNet.Mgr
{
    public class NetSocket<T, K> where T:NetSession<K>,new() where K:NetMsg
    {
        private Socket skt = null;
        public T session = null;
        public int backLog = 10;
        List<T> sessionLst = new List<T>();

        public NetSocket()
        {
            //指定Socket属性
            skt = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        #region Server
        public void StartAsServer(string ip,int port)
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
            }
            catch (Exception e)
            {
                NetLogger.LogMsg(e.Message, LogLevel.Error);
            }
            skt.BeginAccept(new AsyncCallback(ClientConnectCB), skt);
        }


        #endregion

        #region Client
        public void StartAsClient(string ip,int port)
        {
            try
            {
                skt.BeginConnect(new IPEndPoint(IPAddress.Parse(ip), port), new AsyncCallback(ServerConnectCB), skt);
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
            }
            catch (Exception e)
            {
                NetLogger.LogMsg(e.Message, LogLevel.Error);
            }
        }

        #endregion

        public void Close()
        {
            if (skt!=null)
            {
                skt.Close();
            }
        }

        public void SetLog(bool log=true,Action<string,int> logCB = null)
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


    }
}
