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
    public class EventCenter : ISingleton<EventCenter>
    {
        /// <summary>
        /// 线程安排的字典，避免以后多线程环境下出现问题
        /// </summary>
        public static ConcurrentDictionary<EventType, Delegate> evetDic = new ConcurrentDictionary<EventType, Delegate>();


        #region 特性实现的事件系统

        public readonly Dictionary<EventType, List<IEvent>> allEvents = new Dictionary<EventType, List<IEvent>>();

        public readonly Dictionary<DLLType, Assembly> assemblies = new Dictionary<DLLType, Assembly>();
        public readonly UnOrderMultiMap<Type, Type> types = new UnOrderMultiMap<Type, Type>();

        public void Add(DLLType dllType, Assembly assembly)
        {
            this.assemblies[dllType] = assembly;
            this.types.Clear();//清空

            foreach (Assembly val in this.assemblies.Values)
            {
                foreach (Type type in val.GetTypes())
                {
                    object[] objects = type.GetCustomAttributes(typeof(BaseAttribute), false);
                    if (objects.Length == 0)
                    {
                        continue;
                    }

                    BaseAttribute baseAttribute = (BaseAttribute)objects[0];
                    this.types.Add(baseAttribute.attributeType, type);
                }

                Console.WriteLine("扫描特性成功");
            }

        }

        /// <summary>
        /// 基于特性的注册事件的方法
        /// </summary>
        /// <param name="eventType"></param>
        public void RegisterEvent(EventType eventType)
        {
            foreach (Type type in types[typeof(EventAttribute)])
            {
                object[] attrs = type.GetCustomAttributes(typeof(EventAttribute), false);

                foreach (object attr in attrs)
                {
                    EventAttribute eventAttribute = (EventAttribute)attr;

                    object obj = Activator.CreateInstance(type);

                    IEvent ievent = obj as IEvent;

                    if (ievent == null)
                    {
                        Console.WriteLine($"{obj.GetType().Name}没有继承IEvent");
                    }


                    Console.WriteLine("eventId:" + eventType + ":" + ievent);
                    if (!this.allEvents.ContainsKey(eventType))
                    {
                        this.allEvents.Add(eventType, new List<IEvent>());
                    }
                    this.allEvents[eventType].Add(ievent);
                }

            }
        }

        /// <summary>
        /// 基于特性的移除事件的方法
        /// </summary>
        /// <param name="type"></param>
        /// <param name="e"></param>
        public void RemoveEvent(EventType type,IEvent e)
        {
            if (!this.allEvents.ContainsKey(type))
            {
                this.allEvents.Remove(type);
            }
        }

        #region Run
        /// <summary>
        /// 基于特性的广播事件的方法
        /// </summary>
        /// <param name="type"></param>
        public void RunEvent(EventType type)
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
                    iEvent?.Handle();
                }
                catch (Exception e)
                {
                    NetLogger.LogMsg(e.ToString());
                }
            }
        }

        /// <summary>
        /// 基于特性的广播事件的方法
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <param name="type"></param>
        /// <param name="a"></param>
        public void RunEvent<A>(EventType type, A a)
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
                    iEvent?.Handle();
                }
                catch (Exception e)
                {
                    NetLogger.LogMsg(e.ToString());
                }
            }
        }

        /// <summary>
        /// 基于特性的广播事件的方法
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <typeparam name="B"></typeparam>
        /// <param name="type"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public void RunEvent<A, B>(EventType type, A a, B b)
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

        public void RunEvent<A, B, C>(EventType type, A a, B b, C c)
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

        #endregion

        #region 普通的委托调用

        /// <summary>
        /// 添加事件
        /// </summary>
        /// <param name="key"></param>
        /// <param name="callBack"></param>
        private static void OnListenerAdding(EventType eventType, Delegate callBack)
        {
            if (!evetDic.ContainsKey(eventType))
            {
                evetDic.TryAdd(eventType, null);
            }
            Delegate d = evetDic[eventType];
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
            if (evetDic.ContainsKey(eventType))
            {
                Delegate d = evetDic[eventType];
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
            if (evetDic[eventType] == null)
            {
                Delegate removeDele;
                evetDic.TryRemove(eventType, out removeDele);
            }
        }

        #region 重载函数
        /// <summary>
        /// 普通的添加委托的方法
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="callBack"></param>
        public static void AddListener(EventType eventType, CallBack callBack)
        {
            OnListenerAdding(eventType, callBack);
            evetDic[eventType] = (CallBack)evetDic[eventType] + callBack;
        }


        /// <summary>
        /// 普通的添加委托的方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="eventType"></param>
        /// <param name="callBack"></param>
        public static void AddListener<T>(EventType eventType, CallBack<T> callBack)
        {
            OnListenerAdding(eventType, callBack);
            evetDic[eventType] = (CallBack<T>)evetDic[eventType] + callBack;
        }

        /// <summary>
        /// 普通的添加委托的方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="X"></typeparam>
        /// <param name="eventType"></param>
        /// <param name="callBack"></param>
        public static void AddListener<T, X>(EventType eventType, CallBack<T, X> callBack)
        {
            OnListenerAdding(eventType, callBack);
            evetDic[eventType] = (CallBack<T, X>)evetDic[eventType] + callBack;
        }
        /// <summary>
        /// 普通的添加委托的方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="X"></typeparam>
        /// <typeparam name="Y"></typeparam>
        /// <param name="eventType"></param>
        /// <param name="callBack"></param>
        public static void AddListener<T, X, Y>(EventType eventType, CallBack<T, X, Y> callBack)
        {
            OnListenerAdding(eventType, callBack);
            evetDic[eventType] = (CallBack<T, X, Y>)evetDic[eventType] + callBack;
        }

        /// <summary>
        /// 普通的添加委托的方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="X"></typeparam>
        /// <typeparam name="Y"></typeparam>
        /// <typeparam name="Z"></typeparam>
        /// <param name="eventType"></param>
        /// <param name="callBack"></param>
        public static void AddListener<T, X, Y, Z>(EventType eventType, CallBack<T, X, Y, Z> callBack)
        {
            OnListenerAdding(eventType, callBack);
            evetDic[eventType] = (CallBack<T, X, Y, Z>)evetDic[eventType] + callBack;
        }
        /// <summary>
        /// 普通的添加委托的方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="X"></typeparam>
        /// <typeparam name="Y"></typeparam>
        /// <typeparam name="Z"></typeparam>
        /// <typeparam name="W"></typeparam>
        /// <param name="eventType"></param>
        /// <param name="callBack"></param>
        public static void AddListener<T, X, Y, Z, W>(EventType eventType, CallBack<T, X, Y, Z, W> callBack)
        {
            OnListenerAdding(eventType, callBack);
            evetDic[eventType] = (CallBack<T, X, Y, Z, W>)evetDic[eventType] + callBack;
        }
        /// <summary>
        /// 普通的添加委托的方法
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="callBack"></param>
        public static void RemoveListener(EventType eventType, CallBack callBack)
        {
            OnListernerRemoving(eventType, callBack);
            evetDic[eventType] = (CallBack)evetDic[eventType] - callBack;
            OnListenerRemoved(eventType);
        }

        /// <summary>
        /// 普通的添加委托的方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="eventType"></param>
        /// <param name="callBack"></param>
        public static void RemoveListener<T>(EventType eventType, CallBack<T> callBack)
        {
            OnListernerRemoving(eventType, callBack);
            evetDic[eventType] = (CallBack<T>)evetDic[eventType] - callBack;
            OnListenerRemoved(eventType);
        }

        /// <summary>
        /// 普通的添加委托的方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="X"></typeparam>
        /// <param name="eventType"></param>
        /// <param name="callBack"></param>
        public static void RemoveListener<T, X>(EventType eventType, CallBack<T, X> callBack)
        {
            OnListernerRemoving(eventType, callBack);
            evetDic[eventType] = (CallBack<T, X>)evetDic[eventType] - callBack;
            OnListenerRemoved(eventType);
        }

        /// <summary>
        /// 普通的添加委托的方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="X"></typeparam>
        /// <typeparam name="Y"></typeparam>
        /// <param name="eventType"></param>
        /// <param name="callBack"></param>
        public static void RemoveListener<T, X, Y>(EventType eventType, CallBack<T, X, Y> callBack)
        {
            OnListernerRemoving(eventType, callBack);
            evetDic[eventType] = (CallBack<T, X, Y>)evetDic[eventType] - callBack;
            OnListenerRemoved(eventType);
        }

        /// <summary>
        /// 普通的添加委托的方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="X"></typeparam>
        /// <typeparam name="Y"></typeparam>
        /// <typeparam name="Z"></typeparam>
        /// <param name="eventType"></param>
        /// <param name="callBack"></param>
        public static void RemoveListener<T, X, Y, Z>(EventType eventType, CallBack<T, X, Y, Z> callBack)
        {
            OnListernerRemoving(eventType, callBack);
            evetDic[eventType] = (CallBack<T, X, Y, Z>)evetDic[eventType] - callBack;
            OnListenerRemoved(eventType);
        }

        /// <summary>
        /// 普通的添加委托的方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="X"></typeparam>
        /// <typeparam name="Y"></typeparam>
        /// <typeparam name="Z"></typeparam>
        /// <typeparam name="W"></typeparam>
        /// <param name="eventType"></param>
        /// <param name="callBack"></param>
        public static void RemoveListener<T, X, Y, Z, W>(EventType eventType, CallBack<T, X, Y, Z, W> callBack)
        {
            OnListernerRemoving(eventType, callBack);
            evetDic[eventType] = (CallBack<T, X, Y, Z, W>)evetDic[eventType] - callBack;
            OnListenerRemoved(eventType);
        }

        /// <summary>
        /// 普通的广播委托的方法
        /// </summary>
        /// <param name="eventType"></param>
        public static void Broadcast(EventType eventType)
        {
            Delegate d;
            if (evetDic.TryGetValue(eventType, out d))
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

        /// <summary>
        /// 普通的广播委托的方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="eventType"></param>
        /// <param name="arg"></param>
        public static void Broadcast<T>(EventType eventType, T arg)
        {
            Delegate d;
            if (evetDic.TryGetValue(eventType, out d))
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

        /// <summary>
        /// 普通的广播委托的方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="X"></typeparam>
        /// <param name="eventType"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        public static void Broadcast<T, X>(EventType eventType, T arg1, X arg2)
        {
            Delegate d;
            if (evetDic.TryGetValue(eventType, out d))
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

        /// <summary>
        /// 普通的广播委托的方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="X"></typeparam>
        /// <typeparam name="Y"></typeparam>
        /// <param name="eventType"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        public static void Broadcast<T, X, Y>(EventType eventType, T arg1, X arg2, Y arg3)
        {
            Delegate d;
            if (evetDic.TryGetValue(eventType, out d))
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

        /// <summary>
        /// 普通的广播委托的方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="X"></typeparam>
        /// <typeparam name="Y"></typeparam>
        /// <typeparam name="Z"></typeparam>
        /// <param name="eventType"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        /// <param name="arg4"></param>
        public static void Broadcast<T, X, Y, Z>(EventType eventType, T arg1, X arg2, Y arg3, Z arg4)
        {
            Delegate d;
            if (evetDic.TryGetValue(eventType, out d))
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

        /// <summary>
        /// 普通的广播委托的方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="X"></typeparam>
        /// <typeparam name="Y"></typeparam>
        /// <typeparam name="Z"></typeparam>
        /// <typeparam name="W"></typeparam>
        /// <param name="eventType"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        /// <param name="arg4"></param>
        /// <param name="arg5"></param>
        public static void Broadcast<T, X, Y, Z, W>(EventType eventType, T arg1, X arg2, Y arg3, Z arg4, W arg5)
        {
            Delegate d;
            if (evetDic.TryGetValue(eventType, out d))
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

        #endregion


    }
}
