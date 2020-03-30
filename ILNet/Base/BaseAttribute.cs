using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ILNet.Base
{
    [AttributeUsage(AttributeTargets.Class,AllowMultiple =true)]
    public class BaseAttribute : Attribute
    {
        public Type attributeType { get; }

        public BaseAttribute()
        {
            this.attributeType = this.GetType();
        }

    }
}
