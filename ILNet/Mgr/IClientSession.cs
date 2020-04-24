using ILNet.Chat;
using ILNet.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ILNet.Mgr
{

    public class IClientSession<T, R> : ISocket where T : ISession<R>, new() where R : NetMsg
    {

        public T Client;
        public event Action ConnectErrorEvent;

        public override void StartCreate(string ip, int port)
        {
            try
            {
                Client = new T();
                //开始连接
                skt.BeginConnect(IPAddress.Parse(ip), port, ConnectAsync, Client);
                NetLogger.LogMsg("\n客户端启动成功!\n准备连接服务端......", LogLevel.Info);
            }
            catch (Exception e)
            {
                NetLogger.LogMsg($"启动客户端错误：{e.Message}", LogLevel.Error);
            }
        }

        public override void ConnectAsync(IAsyncResult ar)
        {
            try
            {
                skt.EndConnect(ar);
                
                //连接完成开始接收数据
                Client.StartRcvData(skt, ()=> { Client = null; });
            }
            catch (Exception e)
            {
                ConnectErrorEvent?.Invoke();
                NetLogger.LogMsg($"客户端异步连接错误:{e.Message}", LogLevel.Error);
            }
        }

    
    }
}
