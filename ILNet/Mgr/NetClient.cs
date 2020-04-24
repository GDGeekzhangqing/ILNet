using ILNet.Chat;
using ILNet.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ILNet.Mgr
{
    public class NetClient<T, K> where T : NetSession<K>, new() where K : NetMsg
    {

        private Socket skt = null;
        public T session = null;
        public int backLog = 10;
        List<T> sessionLst = new List<T>();

        public NetClient()
        {
            //指定Socket属性
            skt = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void StartAsClient(string ip, int port)
        {
            try
            {
                skt.BeginConnect(new IPEndPoint(IPAddress.Parse(ip), port), new AsyncCallback(ServerConnectCB), skt);
                NetLogger.LogMsg("\nClient Start Success!\nConnecting To Server......", LogLevel.Info);
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
                Thread server = new Thread(new ThreadStart(SendHeardBeatCB));
                server.IsBackground = true;
                server.Start();

            }
            catch (Exception e)
            {
                NetLogger.LogMsg(e.Message, LogLevel.Error);
            }
        }


        /// <summary>
        /// 开始发送心跳包
        /// </summary>
        private void SendHeardBeatCB()
        {
            session.ReviceHeartBeat();
            NetLogger.LogMsg("开始发送心跳包...");
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

    }
}
