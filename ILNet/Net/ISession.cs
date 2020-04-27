using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ILNet.Tools;


namespace ILNet.Net
{
    public abstract class ISession<T> where T : NetMsg
    {
        public Socket skt;
        public int sessionID = 0;

        /// <summary>
        /// 连接成功时的回调
        /// </summary>
        public event Action OnStartConnectEvent;

        /// <summary>
        /// 接收数据时的回调
        /// </summary>
        public event Action<T> OnReciveMsgEvent;

        /// <summary>
        /// 关闭连接时的回调
        /// </summary>
        public event Action OnDisReciveEvent;

        public NetPkg pack = new NetPkg();

        /// <summary>
        /// 开始数据接收
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="close"></param>
        public void StartReciveData(Socket socket, Action close = null)
        {
            try
            {
                // 初始化赋值
                skt = socket;
                OnDisReciveEvent += close;

                //回调开启连接事件
                OnStartConnected();
                OnStartConnectEvent?.Invoke();
                //首先是接收头4个字节确认包长
                //4可能太小了
                pack.headBuff = new byte[4];
                skt.BeginReceive(
                    pack.headBuff,
                    0,
                    4,
                    SocketFlags.None,
                    ReciveHeadData,
                    null);
            }
            catch (Exception e)
            {
                NetLogger.LogMsg($"开始接收数据错误：{e}", LogLevel.Error);
            }
        }

        /// <summary>
        /// 接收包头数据
        /// </summary>
        /// <param name="ar"></param>
        protected void ReciveHeadData(IAsyncResult ar)
        {
            try
            {
                //接收数据长度等于0意味着断开连接 
                //在断开的时候 异步会回调一次 直接调用EndReceive 会报错
                if (skt == null)
                    return;
                if (skt.Available == 0)
                {
                    Clear();
                    return;
                }

                int len = skt.EndReceive(ar);
                if (len > 0)
                {
                    pack.headIndex += len;
                    //这里如果是小于4的就是凑不成 就是分包了 要继续接收
                    if (pack.headIndex < pack.headLen)
                    {
                        //                                                                           
                        skt.BeginReceive(
                            pack.headBuff,
                            pack.headIndex,
                            pack.headLen - pack.headIndex,
                            SocketFlags.None,
                            ReciveHeadData,
                            null);
                    }
                    //这里已经取出长度了
                    else
                    {
                        //赋值从那四个字节获取的byte[]的长度
                        pack.InitBodyBuff();
                        //进行真正的数据接收处理
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
                NetLogger.LogMsg($"接收：{e}", LogLevel.Error);
            }
        }

        /// <summary>
        /// 接收包体数据
        /// </summary>
        /// <param name="ar"></param>
        protected void RcvBodyData(IAsyncResult ar)
        {
            try
            {
                //结束接收获取长度
                int len = skt.EndReceive(ar);

                if (len > 0)
                {
                    pack.bodyIndex += len;

                    //这里是如果接收到的包长和原本获取到的长度小，就是分包了 需要再次进行接收剩下的
                    if (pack.bodyIndex < pack.bodyLen)
                    {

                        skt.BeginReceive(
                            pack.bodyBuff,
                            pack.bodyIndex,
                            pack.headLen - pack.bodyIndex,
                            SocketFlags.None,
                            RcvBodyData,
                            null);
                    }
                    //已经接完一组数据了
                    else
                    {
                        //这里就可以进行回调函数了
                        T msg = AnalysisMsg.DeSerialize<T>(pack.bodyBuff);
                        OnReciveMsg(msg);
                        OnReciveMsgEvent?.Invoke(msg);

                        //开始新一轮的从上往下接收了
                        pack.ResetData();
                        pack.headBuff = new byte[4];
                        skt.BeginReceive(
                            pack.headBuff,
                            0,
                            4,
                            SocketFlags.None,
                            ReciveHeadData,
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
                NetLogger.LogMsg($"ReciveDataError：{e}", LogLevel.Error);
            }

        }
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
            //创建流 准备异步写入发送
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
                        SendDataAsync,
                         ns);
                }
            }
            catch (Exception e)
            {
                NetLogger.LogMsg($"SendDataError：{e}", LogLevel.Error);
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
                NetLogger.LogMsg($"SendDataAsyncError：{e}", LogLevel.Error);
            }
        }
        #endregion

        /// <summary>
        /// 网络关闭
        /// </summary>
        public void Clear()
        {
            OnDisConnected();
            OnDisReciveEvent?.Invoke();
            skt.Close();
            skt = null;
        }

        /// <summary>
        /// 连接成功
        /// </summary>
        protected virtual void OnStartConnected()
        {
            NetLogger.LogMsg($"客户端{sessionID}上线，上线时间：", LogLevel.Info);
        }

        /// <summary>
        /// 接收网络消息
        /// </summary>
        /// <param name="data"></param>
        protected virtual void OnReciveMsg(T data)
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
    }
}
