using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ILNet.Tools;

namespace ILNet.Net
{
    public class IServerSession<T, R> : ISocket where T : ISession<R>, new() where R : NetMsg
    {
        public List<T> SessionList = new List<T>();//储存会话管理的
        public int NetListen = 10;//监听数

        public event Action<T> AddSessionEvent;
        public event Action<T> RemoveSessionEvent;

        public override void StartCreate(string ip, int port)
        {
            try
            {
                //绑定地址
                skt.Bind(new IPEndPoint(IPAddress.Parse(ip), port));
                //设置监听数
                skt.Listen(NetListen);
                //开始监听
                skt.BeginAccept(ConnectAsync, null);
                NetLogger.LogMsg("建立服务器........");
            }
            catch (Exception e)
            {
                NetLogger.LogMsg($"创建服务端连接错误：{e}" ,LogLevel.Error);
            }
        }

        //异步回调
        public override void ConnectAsync(IAsyncResult ar)
        {
            try
            {
                T client = new T();
                //这里结束接收 获取刚刚连接的socket
                Socket sk = skt.EndAccept(ar);

                //开始监听  第二个是加入结束事件
                client.StartReciveData(sk,
                    () =>
                    {
                        RemoveSession(client);
                        RemoveSessionEvent?.Invoke(client);
                        SessionList.Remove(client);
                    });
                //存储会话
                SessionList.Add(client);
                AddSession(client);

                //添加委托监听
                AddSessionEvent?.Invoke(client);

                //开始新一轮接收连接
                skt.BeginAccept(ConnectAsync, null);
            }
            catch (Exception e)
            {
                NetLogger.LogMsg($"服务端连接回调错误：{e}", LogLevel.Error);
                skt?.BeginAccept(ConnectAsync, null);
            }
        }

        /// <summary>
        /// 添加会话
        /// </summary>
        /// <param name="obj"></param>
        public virtual void AddSession(T obj)
        {

        }

        /// <summary>
        /// 移除会话
        /// </summary>
        /// <param name="obj"></param>
        public virtual void RemoveSession(T obj)
        {

        }
    }
}
