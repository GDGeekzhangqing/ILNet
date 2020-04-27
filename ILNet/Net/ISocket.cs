using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ILNet.Tools;

namespace ILNet.Net
{
    public abstract class ISocket
    {

        public Socket skt;

        public ISocket()
        {
            // 指定地址类型 套接字类型 协议
            skt = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        /// <summary>
        /// 创建Socket连接实例
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public abstract void StartCreate(string ip, int port);

        /// <summary>
        /// 建立的回调
        /// </summary>
        /// <param name="ar"></param>
        public abstract void ConnectAsync(IAsyncResult ar);

        /// <summary>
        /// 日志输出
        /// </summary>
        /// <param name="log"></param>
        /// <param name="logCB"></param>
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
