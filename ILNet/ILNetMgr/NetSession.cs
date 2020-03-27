using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ILNet.INetChat;
using ILNet.ILNetHelper;

namespace ILNet.ILNetMgr
{
    public abstract class NetSession<T>where T : NetMsg
    {

        public int sessionID = 0;


        private Socket skt;
        /// <summary>
        /// 关闭回调的委托
        /// </summary>
        private Action closeCB;

        #region Recevie

        /// <summary>
        /// 开始接收网络数据
        /// </summary>
        /// <param name="skt"></param>
        /// <param name="closeCB"></param>
        public void StartRcvData(Socket skt,Action closeCB)
        {
            try
            {
                this.skt = skt;
                this.closeCB = closeCB;

                OnConnected();

                NetPkg pack = new NetPkg();
                //开始异步接收数据
                skt.BeginReceive(
                    pack.headBuff,
                    0,
                    pack.headLen,
                    SocketFlags.None,
                    new AsyncCallback(RcvHeadData),
                    pack );
          
            }
            catch (Exception e)
            {
                NetLogger.LogMsg("StartRcvData:" + e.Message, LogLevel.Error);
            }
        }
      
        /// <summary>
        /// 接收包头数据
        /// </summary>
        /// <param name="ar"></param>
       private void RcvHeadData(IAsyncResult ar)
        {
            try
            {
                NetPkg pack = (NetPkg)ar.AsyncState;
                if (skt.Available==0)
                {
                    OnDisConnected();
                    Clear();
                    return;
                }

                int len = skt.EndReceive(ar);
                if (len>0)
                {
                    pack.headIndex += len;
                    if (pack.headIndex<pack.headLen)
                    {
                        skt.BeginReceive(
                            pack.headBuff,
                            pack.headIndex,
                            pack.headLen - pack.headIndex,
                            SocketFlags.None,
                            new AsyncCallback(RcvHeadData),
                            pack);
                    }
                    else
                    {
                        pack.InitBodyBuff();
                        skt.BeginReceive(pack.bodyBuff,
                            0,
                            pack.bodyLen,
                            SocketFlags.None,
                            new AsyncCallback(RcvBodyData),
                            pack);
                    }
                }
                else
                {
                    OnDisConnected();
                    Clear();
                }
            }
            catch (Exception e)
            {
                NetLogger.LogMsg("RcvHeadError:" + e.Message, LogLevel.Error);
            }
        }

        /// <summary>
        /// 接收包体数据
        /// </summary>
        /// <param name="ar"></param>
        private void RcvBodyData(IAsyncResult ar)
        {
            try
            {
                NetPkg pack = (NetPkg)ar.AsyncState;
                int len = skt.EndReceive(ar);
                if (len>0)
                {
                    pack.bodyIndex += len;
                    if (pack.bodyIndex<pack.bodyLen)
                    {
                        skt.BeginReceive(pack.bodyBuff,
                            pack.bodyIndex,
                            pack.headLen - pack.bodyIndex,
                            SocketFlags.None,
                            new AsyncCallback(RcvBodyData),
                            pack
                            );
                    }
                    else
                    {
                        T msg = AnalysisMsg.DeSerialize<T>(pack.bodyBuff);
                        OnReciveMsg(msg);

                        //循环接收
                        pack.ResetData();
                        skt.BeginReceive(
                            pack.headBuff,
                            0,
                            pack.headLen,
                            SocketFlags.None,
                            new AsyncCallback(RcvHeadData),
                            pack);
                    }
                }
                else
                {
                    OnDisConnected();
                    Clear();
                }
            }
            catch (Exception e)
            {
                NetLogger.LogMsg("RcvBodyError:" + e.Message, LogLevel.Error);
            }
        }

        #endregion

        #region Send
        /// <summary>
        /// 发送网络数据
        /// </summary>
        /// <param name="msg"></param>
        public void SendMsg(T msg)
        {
            byte[] data = AnalysisMsg.PackLenInfo(AnalysisMsg.Serialize<T>(msg));
            SendMsg(data);
        }
        /// <summary>
        /// 发送网络数据
        /// </summary>
        /// <param name="data"></param>
        public void SendMsg(byte[] data)
        {
            NetworkStream ns = null;
            try
            {
                ns = new NetworkStream(skt);
                if (ns.CanWrite)
                {
                    ns.BeginWrite(
                        data,
                        0,
                        data.Length,
                        new AsyncCallback(SendCB),
                        ns);
                }
            }
            catch (Exception e)
            {
                NetLogger.LogMsg("SndMsgError:" + e.Message, LogLevel.Error);
            }
        }

        /// <summary>
        /// 发送ACK消息的接口
        /// </summary>
        /// <param name="ackPackage"></param>
        /// <param name="endPoint"></param>
        public void SendACK()
        {
            NetLogger.LogMsg("回复客户端收到消息了!");
            SendMsg();
        }

        /// <summary>
        /// 发送网络数据后的回调
        /// </summary>
        /// <param name="ar"></param>
        private void SendCB(IAsyncResult ar)
        {
            NetworkStream ns = (NetworkStream)ar.AsyncState;
            try
            {
                ns.EndWrite(ar);
                ns.Flush();
                ns.Close();
            }
            catch (Exception e)
            {
               NetLogger.LogMsg("SndMsgError:" + e.Message, LogLevel.Error);
            }
        }

        #endregion

        #region 释放网络资源

        /// <summary>
        /// 释放网络资源
        /// </summary>
        private void Clear()
        {
            if (closeCB!=null)
            {
                closeCB();
            }
            skt.Close();
        }
        #endregion

        /// <summary>
        /// 连接网络
        /// </summary>
        protected virtual void OnConnected()
        {
            NetLogger.LogMsg("New Seesion Connected.", LogLevel.Info);
        }
        /// <summary>
        /// 接收网络消息
        /// </summary>
        /// <param name="msg"></param>
        protected virtual void OnReciveMsg(T msg)
        {
            NetLogger.LogMsg("Receive Network Message.", LogLevel.Info);
        }
        /// <summary>
        /// 断开网络连接
        /// </summary>
        protected virtual void OnDisConnected()
        {
            NetLogger.LogMsg("Session Disconnected.", LogLevel.Info);
        }

    }
}
