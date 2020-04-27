using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ILNet.Net
{
   public class NetPkg
    {
        /// <summary>
        /// 包头长度
        /// </summary>
        public int headLen = 4;

        public byte[] headBuff = null;

        /// <summary>
        /// 记录分包时，接收到单个包的个数，判断还需接收多少个
        /// </summary>
        public int headIndex = 0;

        /// <summary>
        /// 数据包长度
        /// </summary>
        public int bodyLen = 0;

        public byte[] bodyBuff = null;

      
        public int bodyIndex = 0;

        public NetPkg()
        {
            headBuff = new byte[4];
        }

        /// <summary>
        /// 获取四个字节组成的int长度
        /// </summary>
        public void InitBodyBuff()
        {
            bodyLen = BitConverter.ToInt32(headBuff, 0);
            bodyBuff = new byte[bodyLen];
        }

        /// <summary>
        /// 重置包体数据
        /// </summary>
        public void ResetData()
        {
            headIndex = 0;
            bodyLen = 0;
            bodyBuff = null;
            bodyIndex = 0;
        }

    }
}
