using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ILNet.ILNetBase;

namespace ILNet.ILNetEvent
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class EventAttribute : BaseAttribute
    {
        public string Type { get; }

        public EventAttribute(string type)
        {
            Type = type;
        }

    }
}
