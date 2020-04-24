using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ILNet.Tools;

namespace ILNet.Mgr
{
    public abstract class ISocket
    {
        public Socket skt ;

        public ISocket()
        {
            //指定Socket属性
            skt = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        /// <summary>
        /// 开始创建Socket实例
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public abstract void StartCreate(string ip, int port);

        /// <summary>
        /// 成功创建Socket时的回调
        /// </summary>
        /// <param name="ar"></param>
        public abstract void ConnectAsync(IAsyncResult ar);

        /// <summary>
        /// 关闭Socket
        /// </summary>
        public void Close()
        {
            if (skt != null)
            {
                skt.Close();
            }
        }

        /// <summary>
        /// 打印日志
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
