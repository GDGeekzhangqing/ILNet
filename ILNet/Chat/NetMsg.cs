using System;
using ILNet.Tools;

namespace ILNet.Chat
{
  
  [Serializable]
   public abstract class NetMsg
    {
        /// <summary>
        /// 序列
        /// </summary>
        public int seq;
        /// <summary>
        /// 指令
        /// </summary>
        public int cmd;
        /// <summary>
        /// 错误码
        /// </summary>
        public int err;

    }
}
