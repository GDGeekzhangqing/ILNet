using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ILNet.Chat;
using ILNet.Tools;

namespace ILNet.Mgr
{
    public abstract class NetSession<T> where T : NetMsg
    {
        public Socket skt;
  
        public int sessionID = 0;

        /// <summary>
        /// 用于指定client的索引
        /// </summary>
        protected Int32 clientID;

        /// <summary>
        /// 当前客户端是否离线
        /// </summary>
        protected Boolean isOffLine;

        /// <summary>
        /// 关闭回调的委托
        /// </summary>
        private Action closeCB;

        /// <summary>
        /// 给服务端使用的客户端连接对象字典
        /// </summary>
        protected Dictionary<Int32, ClientInfo> clientDic = new Dictionary<int, ClientInfo>();

        /// <summary>
        /// 对外公布的心跳包更新时间
        /// </summary>
        protected int heardbeatTime = 3;




        #region Recevie

        /// <summary>
        /// 开始接收网络数据
        /// </summary>
        /// <param name="skt"></param>
        /// <param name="closeCB"></param>
        public void StartRcvData(Socket skt, Action closeCB)
        {
            try
            {
                this.skt = skt;
                this.closeCB = closeCB;

                //连接成功的回调
                OnConnected();

                NetPkg pack = new NetPkg();
                //开始异步接收数据
                skt.BeginReceive(
                    pack.headBuff,
                    0,
                    pack.headLen,
                    SocketFlags.None,
                    new AsyncCallback(RcvHeadData),
                    pack);
  
                Console.WriteLine("接收数据：" + skt.ToString());
            }
            catch (Exception e)
            {
                NetLogger.LogMsg("开始接收数据错误:" + e.Message, LogLevel.Error);
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
                //避免断开连接时，异步回调调用EndReceive会报错
                /*if (skt==null)
                {
                    NetLogger.LogMsg("当前对应客户端不存在，直接返回");
                    return;
                }*/

                if (skt==null)
                    return;


                NetPkg pack = (NetPkg)ar.AsyncState;
                if (skt.Available == 0)
                {
                    OnDisConnected();
                    Clear();
                    return;
                }
                NetLogger.LogMsg("开始结束异步读取");
                int len = skt.EndReceive(ar);
                if (len > 0)
                {
                    pack.headIndex += len;
                    //如果是小于4的就是凑不成一个包头，就是要分包继续接收
                    if (pack.headIndex < pack.headLen)
                    {
                        //接收数据
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
                        //设置byte[]的长度
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
                NetLogger.LogMsg("接收包头数据错误:" + e.Message, LogLevel.Error);
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
                if (len > 0)
                {
                    pack.bodyIndex += len;
                    if (pack.bodyIndex < pack.bodyLen)
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
                NetLogger.LogMsg("接收包体数据错误:" + e.Message, LogLevel.Error);
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
            //创建流，准备异步写入发送
            NetworkStream ns = null;
            try
            {
                //指定写入的socket
                ns = new NetworkStream(skt);
                //判断是否可以支持写入消息
                if (ns.CanWrite)
                {
                    //开始异步写入
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
                NetLogger.LogMsg("发送数据错误:" + e.Message, LogLevel.Error);
            }
        }

        /// <summary>
        /// 这里是异步写入回调
        /// </summary>
        /// <param name="ar"></param>
        protected void SendDataAsync(IAsyncResult ar)
        {
            //拿到写入时候的流
            NetworkStream network = (NetworkStream)ar.AsyncState;
            try
            {
                //结束写入 就是发送了  然后进行关闭流
                network.EndWrite(ar);
                network.Flush();
                network.Close();

            }
            catch (Exception e)
            {
                NetLogger.LogMsg("异步写入数据出错：" + e, LogLevel.Error);
            }
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
                NetLogger.LogMsg("发送数据回调错误:" + e.Message, LogLevel.Error);
            }
        }

        #endregion

        #region 释放网络资源

     
        /// <summary>
        /// 释放网络资源
        /// </summary>
        public void Clear()
        {
            if (closeCB != null)
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
            NetLogger.LogMsg($"客户端{sessionID}上线，上线时间：", LogLevel.Info);
        }

        /// <summary>
        /// 接收网络消息
        /// </summary>
        /// <param name="msg"></param>
        protected virtual void OnReciveMsg(T msg)
        {

            //更新心跳包    
            NetLogger.LogMsg("接收网络消息.", LogLevel.Info);
        }

        /// <summary>
        /// 断开网络连接
        /// </summary>
        protected virtual void OnDisConnected()
        {
            NetLogger.LogMsg($"客户端{sessionID}离线，离线时间：\t", LogLevel.Info);
        }

        /// <summary>
        /// 客户端离线提示
        /// </summary>
        /// <param name="clientInfo"></param>
        public virtual  void ClientOfflineEvent(NetAckMsg clientInfo)
        {
            NetLogger.LogMsg($"客户端{sessionID}离线，离线时间：\t");
        }

        /// <summary>
        /// 客户端上线提示
        /// </summary>
        /// <param name="clientInfo"></param>
        public virtual void ClientOnlineEvent(NetAckMsg clientInfo)
        {
            NetLogger.LogMsg($"客户端{sessionID}上线，上线时间：\t{clientInfo.lastHeartTime}");
        }
    }
}
