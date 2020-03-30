using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

using ILNet.Event;
using System.Reflection;
using ILNet.Base;
using ILNet.Tools;

public enum DLLType 
{
    Model,
    Hofix,
    Editor
 
}


namespace ILNet.Event
{
    public class EventCenter:ISingleton<EventCenter>
    {
        /// <summary>
        /// 线程安排的字典，避免以后多线程环境下出现问题
        /// </summary>
        public static ConcurrentDictionary<EventType, Delegate> eventDic = new ConcurrentDictionary<EventType, Delegate>();

        public readonly Dictionary<DLLType, BaseAttribute> assemblies = new Dictionary<DLLType, BaseAttribute>();
        public readonly Dictionary<Type,Type> types = new Dictionary<Type, Type>();
        public readonly Dictionary<string, List<IEvent>> allEvents = new Dictionary<string, List<IEvent>>();
      

        /// <summary>
        /// 添加事件
        /// </summary>
        /// <param name="key"></param>
        /// <param name="callBack"></param>
        private static void OnListenerAdding(EventType eventType, Delegate callBack)
        {
            if (!eventDic.ContainsKey(eventType))
            {
                eventDic.TryAdd(eventType, null);
            }
            Delegate d = eventDic[eventType];
            if (d != null && d.GetType() != callBack.GetType())
            {
                throw new Exception(string.Format("尝试为事件{0}添加不同类型的委托，当前事件所对应的委托是{1}，要添加的委托类型为{2}", eventType, d.GetType(), callBack.GetType()));
            }
        }

        /// <summary>
        /// 移除事件
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="callBack"></param>
        public static void OnListernerRemoving(EventType eventType, Delegate callBack)
        {
            if (eventDic.ContainsKey(eventType))
            {
                Delegate d = eventDic[eventType];
                if (d == null)
                {
                    throw new Exception(string.Format("移除监听错误：事件{0}没有对应的委托", eventType));
                }
                else if (d.GetType() != callBack.GetType())
                {
                    throw new Exception(string.Format("移除监听错误：尝试为事件{0}移除不同类型的委托，当前委托类型为{1}，要移除的委托类型为{2}", eventType, d.GetType(), callBack.GetType()));
                }
            }
        }

        private static void OnListenerRemoved(EventType eventType)
        {
            if (eventDic[eventType] == null)
            {
                Delegate removeDele;
                eventDic.TryRemove(eventType,out removeDele );
            }
        }  

        #region 重载函数
        public static void AddListener(EventType eventType, CallBack callBack)
        {
            OnListenerAdding(eventType, callBack);
            eventDic[eventType] = (CallBack)eventDic[eventType] + callBack;
        }

        public static void AddListener<T>(EventType eventType, CallBack<T> callBack)
        {
            OnListenerAdding(eventType, callBack);
            eventDic[eventType] = (CallBack<T>)eventDic[eventType] + callBack;
        }

        public static void AddListener<T,X>(EventType eventType, CallBack<T,X> callBack)
        {
            OnListenerAdding(eventType, callBack);
            eventDic[eventType] = (CallBack<T,X>)eventDic[eventType] + callBack;
        }
        public static void AddListener<T,X,Y>(EventType eventType, CallBack<T,X,Y> callBack)
        {
            OnListenerAdding(eventType, callBack);
            eventDic[eventType] = (CallBack<T,X,Y>)eventDic[eventType] + callBack;
        }
        public static void AddListener<T,X,Y,Z>(EventType eventType, CallBack<T,X,Y,Z> callBack)
        {
            OnListenerAdding(eventType, callBack);
            eventDic[eventType] = (CallBack<T,X,Y,Z>)eventDic[eventType] + callBack;
        }

        public static void AddListener<T, X, Y, Z,W>(EventType eventType, CallBack<T, X, Y, Z,W> callBack)
        {
            OnListenerAdding(eventType, callBack);
            eventDic[eventType] = (CallBack<T, X, Y, Z,W>)eventDic[eventType] + callBack;
        }

        public static void RemoveListener(EventType eventType,CallBack callBack)
        {
            OnListernerRemoving(eventType, callBack);
            eventDic[eventType] = (CallBack)eventDic[eventType] - callBack;
            OnListenerRemoved(eventType);
        }

        public static void RemoveListener<T>(EventType eventType, CallBack<T> callBack)
        {
            OnListernerRemoving(eventType, callBack);
            eventDic[eventType] = (CallBack<T>)eventDic[eventType] - callBack;
            OnListenerRemoved(eventType);
        }
        public static void RemoveListener<T,X>(EventType eventType, CallBack<T,X> callBack)
        {
            OnListernerRemoving(eventType, callBack);
            eventDic[eventType] = (CallBack<T,X>)eventDic[eventType] - callBack;
            OnListenerRemoved(eventType);
        }
        public static void RemoveListener<T,X,Y>(EventType eventType, CallBack<T,X,Y> callBack)
        {
            OnListernerRemoving(eventType, callBack);
            eventDic[eventType] = (CallBack<T,X,Y>)eventDic[eventType] - callBack;
            OnListenerRemoved(eventType);
        }
        public static void RemoveListener<T,X,Y,Z>(EventType eventType, CallBack<T,X,Y,Z> callBack)
        {
            OnListernerRemoving(eventType, callBack);
            eventDic[eventType] = (CallBack<T,X,Y,Z>)eventDic[eventType] - callBack;
            OnListenerRemoved(eventType);
        }

        public static void RemoveListener<T, X, Y, Z,W>(EventType eventType, CallBack<T, X, Y, Z,W> callBack)
        {
            OnListernerRemoving(eventType, callBack);
            eventDic[eventType] = (CallBack<T, X, Y, Z,W>)eventDic[eventType] - callBack;
            OnListenerRemoved(eventType);
        }

       
        public static void Broadcast(EventType eventType)
        {
            Delegate d;
            if (eventDic.TryGetValue(eventType, out d))
            {
                CallBack callBack = d as CallBack;
                if (callBack != null)
                {
                    callBack();
                }
                else
                {
                    throw new Exception(string.Format("广播事件错误：事件{0}对应委托具有不同的类型", eventType));
                }
            }
        }
 
        public static void Broadcast<T>(EventType eventType, T arg)
        {
            Delegate d;
            if (eventDic.TryGetValue(eventType, out d))
            {
                CallBack<T> callBack = d as CallBack<T>;
                if (callBack != null)
                {
                    callBack(arg);
                }
                else
                {
                    throw new Exception(string.Format("广播事件错误：事件{0}对应委托具有不同的类型", eventType));
                }
            }
        }
      
        public static void Broadcast<T, X>(EventType eventType, T arg1, X arg2)
        {
            Delegate d;
            if (eventDic.TryGetValue(eventType, out d))
            {
                CallBack<T, X> callBack = d as CallBack<T, X>;
                if (callBack != null)
                {
                    callBack(arg1, arg2);
                }
                else
                {
                    throw new Exception(string.Format("广播事件错误：事件{0}对应委托具有不同的类型", eventType));
                }
            }
        }

        public static void Broadcast<T, X, Y>(EventType eventType, T arg1, X arg2, Y arg3)
        {
            Delegate d;
            if (eventDic.TryGetValue(eventType, out d))
            {
                CallBack<T, X, Y> callBack = d as CallBack<T, X, Y>;
                if (callBack != null)
                {
                    callBack(arg1, arg2, arg3);
                }
                else
                {
                    throw new Exception(string.Format("广播事件错误：事件{0}对应委托具有不同的类型", eventType));
                }
            }
        }

        public static void Broadcast<T, X, Y, Z>(EventType eventType, T arg1, X arg2, Y arg3, Z arg4)
        {
            Delegate d;
            if (eventDic.TryGetValue(eventType, out d))
            {
                CallBack<T, X, Y, Z> callBack = d as CallBack<T, X, Y, Z>;
                if (callBack != null)
                {
                    callBack(arg1, arg2, arg3, arg4);
                }
                else
                {
                    throw new Exception(string.Format("广播事件错误：事件{0}对应委托具有不同的类型", eventType));
                }
            }
        }

        public static void Broadcast<T, X, Y, Z, W>(EventType eventType, T arg1, X arg2, Y arg3, Z arg4, W arg5)
        {
            Delegate d;
            if (eventDic.TryGetValue(eventType, out d))
            {
                CallBack<T, X, Y, Z, W> callBack = d as CallBack<T, X, Y, Z, W>;
                if (callBack != null)
                {
                    callBack(arg1, arg2, arg3, arg4, arg5);
                }
                else
                {
                    throw new Exception(string.Format("广播事件错误：事件{0}对应委托具有不同的类型", eventType));
                }
            }
        }

        #endregion

        #region Run
        public void Run(string type)
        {
            List<IEvent> iEvents;
            if (!this.allEvents.TryGetValue(type,out iEvents))
            {
                return;
            }
            foreach (IEvent iEvent in iEvents)
            {
                try
                {
                    iEvent?.Handle();
                }
                catch (Exception e)
                {
                    NetLogger.LogMsg(e.ToString());
                }
            }
        }

        public void Run<A>(string type ,A a)
        {
            List<IEvent> iEvents;
            if (!this.allEvents.TryGetValue(type,out iEvents))
            {
                return;
            }

            foreach (IEvent iEvent in iEvents)
            {
                try
                {
                    iEvent?.Handle();
                }
                catch (Exception e)
                {
                    NetLogger.LogMsg(e.ToString());
                }
            }
        }

        public void Run<A,B>(string type,A a,B b)
        {
            List<IEvent> iEvents;
            if (!this.allEvents.TryGetValue(type, out iEvents))
            {
                return;
            }
            foreach (IEvent iEvent in iEvents)
            {
                try
                {
                    iEvent?.Handle(a, b);
                }
                catch (Exception e)
                {
                    NetLogger.LogMsg(e.ToString());
                }
            }
        }

        public void Run<A,B,C>(string type,A a,B b,C c)
        {
            List<IEvent> iEvents;
            if (!this.allEvents.TryGetValue(type, out iEvents))
            {
                return;
            }
            foreach (IEvent iEvent in iEvents)
            {
                try
                {
                    iEvent?.Handle(a, b, c);
                }
                catch (Exception e)
                {
                   NetLogger.LogMsg(e.ToString());
                }
            }
        }

        #endregion
    }
}
