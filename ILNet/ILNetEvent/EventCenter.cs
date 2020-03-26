using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using ILNet.ILNetHelper;

namespace ILNet.ILNetEvent
{
   public class EventCenter<T,P,X>:ISingleton<EventCenter<T,P,X>> where T:new() where P:class
    {

        public ConcurrentDictionary<X, List<Action<P>>> dic = new ConcurrentDictionary<X, List<Action<P>>>();

        private static ConcurrentDictionary<X, List<Action<P>>> m_EventTable = new ConcurrentDictionary<X, List<Action<P>>>();

        private static void OnListenerAdding(X key,Action<P> callBack)
        {
            if (!m_EventTable.ContainsKey(key))
            {
                m_EventTable[key].Add(callBack);
            }
        }

        public static void OnListernerRemoving(X eventType,Action<P> callBack)
        {
            if (m_EventTable.ContainsKey(eventType))
            {
                List<Action<P>> actions =m_EventTable[eventType];
                actions.Remove(callBack);

                if (actions.Count==0)
                {
                    List<Action<P>> removeActions;
                    m_EventTable.TryRemove(eventType, out removeActions);
                }
            }
        }

    }
}
