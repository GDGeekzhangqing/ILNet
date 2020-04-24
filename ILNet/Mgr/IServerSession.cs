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
    public class IServerSession<T, R> : ISocket where T : ISession<R>, new() where R : NetMsg
    {
        /// <summary>
        /// 储存会话管理的
        /// </summary>
        public List<T> sessionLst = new List<T>();

        /// <summary>
        /// 监听数
        /// </summary>
        public int backLog = 10;

        /// <summary>
        /// 添加会话事件
        /// </summary>
        public event Action<T> AddSessionEvent;

        /// <summary>
        /// 移除会话事件
        /// </summary>
        public event Action<T> RemoveSessionEvent;

        public override void StartCreate(string ip, int port)
        {
            try
            {
                //绑定地址
                skt.Bind(new IPEndPoint(IPAddress.Parse(ip), port));
                //设置监听数
                skt.Listen(backLog);
                //开始新一轮的接收连接
                skt.BeginAccept(ConnectAsync, null);
                NetLogger.LogMsg("\n服务端启动成功!\n等待连接中......", LogLevel.Info);
            }
            catch (Exception e)
            {
                NetLogger.LogMsg($"服务端异步启动错误:{e.Message}", LogLevel.Error);
            }
        }

        public override void ConnectAsync(IAsyncResult ar)
        {
            try
            {
                T client = new T();

                //结束接收时，获取刚刚连接的socket
                Socket sk = skt.EndAccept(ar);

                //开始监听
                client.StartRcvData(sk,
                 () =>
                {
                    RemoveSession(client);
                    //添加移除客户端的监听
                    RemoveSessionEvent?.Invoke(client);
                    //从列表中移除
                    sessionLst.Remove(client);
                });

                //存储会话
                sessionLst.Add(client);
                AddSession(client);
                //添加委托监听
                AddSessionEvent?.Invoke(client);

                //开始新一轮的接收连接
                skt.BeginAccept(ConnectAsync, null);

            }
            catch (Exception e)
            {
                NetLogger.LogMsg($"客户端 异步连接错误:{e.Message}", LogLevel.Error);
                skt.BeginAccept(ConnectAsync, null);
            }

        }

        public virtual void AddSession(T obj)
        {

        }
        public virtual void RemoveSession(T obj)
        {

        }
    }
}
