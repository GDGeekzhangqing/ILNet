using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ILNet.Mgr
{

    public enum SessionType
    {
        ClientSession,
        ServerSession
    }

    // 客户端离线委托
    public delegate void ClientOfflineHandler(ClientInfo client);

    // 客户端上线委托
    public delegate void ClientOnlineHandler(ClientInfo client);

    public class ClientInfo
    {
        /// <summary>
        /// 客户端ID
        /// </summary>
        public Int32 ClientID;

        /// <summary>
        /// 最后心跳时间
        /// </summary>
        public DateTime LastHeartbeatTime;

        /// <summary>
        /// 连接状态
        /// </summary>
        public Boolean State;

    }
}
