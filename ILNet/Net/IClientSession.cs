
using ILNet.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;


namespace ILNet.Net
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
                //开始新一轮的接收连接
                skt.BeginConnect(IPAddress.Parse(ip), port, ConnectAsync, Client);
                NetLogger.LogMsg("正在连接服务器...");
            }
            catch (Exception e)
            {
                NetLogger.LogMsg($"开始创建客户端连接错误：{e}" ,LogLevel.Error);
            }
        }

        public override void ConnectAsync(IAsyncResult ar)
        {
            try
            {
                skt.EndConnect(ar);
                //连接完成开始接收数据
                Client.StartReciveData(skt, () => { Client = null; });

            }
            catch (Exception e)
            {
                ConnectErrorEvent?.Invoke();
               NetLogger.LogMsg ($"客户端创建连接回调错误：{e}",LogLevel.Error);
            }

        }
    }
}
