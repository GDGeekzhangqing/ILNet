using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ILNet.Tools;

namespace ILNet.Base
{
    public class ISystem:ISingleton<ISystem>
    {
        private Action action;

        public virtual void Init()
        {

            NetLogger.LogMsg($"初始化{this.GetType().Name}成功...");
        }

       

    }
}
