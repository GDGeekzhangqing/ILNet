using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ILNet.Chat;
using ILNet.Tools;

namespace ILNet.Mgr
{
    public abstract class ISession<T> where T : NetMsg
    {

        /// <summary>
        /// 对外公布的socket实例
        /// </summary>
        public Socket skt;

        /// <summary>
        /// 对外公布的会话索引
        /// </summary>
        public int sessionID = 0;

        /// <summary>
        /// 当前客户端是否离线
        /// </summary>
        protected Boolean isOffLine;

        /// <summary>
        /// 首次开启连接的事件
        /// </summary>
        public event Action OnConnectEvent;

        /// <summary>
        /// 接收到数据时的委托事件
        /// </summary>
        public event Action<T> OnReciveMsgEvent;

        /// <summary>
        /// 关闭会话时的委托事件
        /// </summary>
        public event Action OnDisConnectEvent;


        public NetPkg pack = new NetPkg();


        #region Recevie

        /// <summary>
        /// 开始接收网络数据
        /// </summary>
        /// <param name="skt"></param>
        /// <param name="closeCB"></param>
        public void StartRcvData(Socket skt, Action closeCB = null)
        {
            try
            {
                this.skt = skt;
                //添加监听
                this.OnDisConnectEvent += closeCB;

                //连接成功的回调
                OnConnected();
                OnConnectEvent?.Invoke();

               // pack.headBuff = new byte[4];  
                //开始异步接收数据
                skt.BeginReceive(
                    pack.headBuff,
                    0,
                    pack.headLen,
                    SocketFlags.None,
                    RcvHeadData,
                    null);

                // NetLogger.LogMsg("接收数据：" + skt.ToString());
            }
            catch (Exception e)
            {
                NetLogger.LogMsg($"开始接收数据错误:{e.Message}", LogLevel.Error);
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
                if (skt == null)
                    return;

                // NetPkg pack = (NetPkg)ar.AsyncState;
                if (skt.Available == 0)
                {
                    //网络关闭后执行的回调         
                    Clear();
                    return;
                }
                //NetLogger.LogMsg("开始结束异步读取");
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
                            RcvHeadData,
                            null);
                    }
                    else
                    {
                        //设置byte[]的长度
                        pack.InitBodyBuff();
                        skt.BeginReceive(
                            pack.bodyBuff,
                            0,
                            pack.bodyLen,
                            SocketFlags.None,
                            RcvBodyData,
                            null);
                    }
                }
                else
                {
                    Clear();
                }
            }
            catch (Exception e)
            {
                NetLogger.LogMsg($"接收包头数据错误:{ e.Message}", LogLevel.Error);
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
                int len = skt.EndReceive(ar);
                if (len > 0)
                {
                    pack.bodyIndex += len;
                    if (pack.bodyIndex < pack.bodyLen)
                    {
                        skt.BeginReceive(
                            pack.bodyBuff,
                            pack.bodyIndex,
                            pack.headLen - pack.bodyIndex,
                            SocketFlags.None,
                            RcvBodyData,
                            null );
                    }
                    else
                    {
                        //接收完一组数据后进行回调
                        T msg = AnalysisMsg.DeSerialize<T>(pack.bodyBuff);
                        OnReciveMsg(msg);
                        OnReciveMsgEvent?.Invoke(msg);

                        //循环接收
                        pack.ResetData();
                        skt.BeginReceive(
                            pack.headBuff,
                            0,
                            pack.headLen,
                            SocketFlags.None,
                            RcvHeadData,
                            null);
                    }
                }
                else
                {
                    Clear();
                }
            }
            catch (Exception e)
            {
                NetLogger.LogMsg($"接收包体数据错误:{e.Message}" , LogLevel.Error);
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
                        SendCB,
                        ns);
                }
            }
            catch (Exception e)
            {
                NetLogger.LogMsg($"发送数据错误:{ e.Message}", LogLevel.Error);
            }
        }


        /// <summary>
        /// 发送网络数据后的回调
        /// </summary>
        /// <param name="ar"></param>
        private void SendCB(IAsyncResult ar)
        {
            //拿到写入时候的流
            NetworkStream ns = (NetworkStream)ar.AsyncState;
            try
            {
                //结束写入 就是发送了  然后进行关闭流
                ns.EndWrite(ar);
                ns.Flush();
                ns.Close();
            }
            catch (Exception e)
            {
                NetLogger.LogMsg($"异步写入数据出错:{e.Message}", LogLevel.Error);
            }
        }

        #endregion

        #region Clear


        /// <summary>
        /// 释放网络资源
        /// </summary>
        public void Clear()
        {
            OnDisConnected();
            OnDisConnectEvent?.Invoke();
            skt.Close();
            skt = null;
            NetLogger.LogMsg("释放网络资源...");
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
            NetLogger.LogMsg($"接收网络消息：{msg}", LogLevel.Info);
        }

        /// <summary>
        /// 断开网络连接
        /// </summary>
        protected virtual void OnDisConnected()
        {
            NetLogger.LogMsg($"客户端{sessionID}离线，离线时间：{DateTime.UtcNow}\t", LogLevel.Info);
        }
    }
}
