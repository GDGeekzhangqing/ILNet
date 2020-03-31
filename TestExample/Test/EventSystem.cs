using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
public enum DLLType
{
    Model,
    Hotfix,
    Editor,
}

public class EventSystem
{



    public readonly Dictionary<EventType, List<IEvent>> allEvents = new Dictionary<EventType, List<IEvent>>();
    public readonly Dictionary<DLLType, Assembly> assemblies = new Dictionary<DLLType, Assembly>();
    public readonly UnOrderMultiMap<Type, Type> types = new UnOrderMultiMap<Type, Type>();
    public readonly Dictionary<EventType, EventAttribute> tempEventArributeLst = new Dictionary<EventType, EventAttribute>();


    #region 特性加反射，ET示例

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

    public void Run<A, B, C>(EventType type, A a, B b, C c)
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
                Console.WriteLine(e);
            }
        }
    }






    #endregion

}

