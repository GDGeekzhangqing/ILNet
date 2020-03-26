using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ILNet.ILNetHelper;

namespace ILNet.ILNetLua
{
    public abstract class LuaMgr : ISingleton<LuaMgr>
    {
        /// <summary>
        /// 初始化
        /// </summary>
        public abstract void Init();

    }
}
