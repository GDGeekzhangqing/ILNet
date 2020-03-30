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

    private readonly Dictionary<long, Component> allComponents = new Dictionary<long, Component>();

    public readonly Dictionary<string, List<IEvent>> allEvents = new Dictionary<string, List<IEvent>>();
    public readonly Dictionary<DLLType, Assembly> assemblies = new Dictionary<DLLType, Assembly>();
    public readonly UnOrderMultiMap<Type, Type> types = new UnOrderMultiMap<Type, Type>();

    private Queue<long> loaders = new Queue<long>();
    private Queue<long> loaders2 = new Queue<long>();


    #region 特性加反射，ET示例

    public void Add(DLLType dllType,Assembly assembly)
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
        }

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
                this.RegisterEvent(eventAttribute.Type, ievent);
            }

            this.Load();
        }
    }
    public void RegisterEvent(string eventId, IEvent e)
    {
        Console.WriteLine("eventId:" + eventId + ":" + e);
        if (!this.allEvents.ContainsKey(eventId))
        {
            this.allEvents.Add(eventId, new List<IEvent>());
        }
        this.allEvents[eventId].Add(e);
    }

    public Assembly Get(DLLType dllType)
    {
        return this.assemblies[dllType];
    }

    public List<Type> GetTypes(Type systemAttributeType)
    {
        if (!this.types.ContainsKey(systemAttributeType))
        {
            return new List<Type>();
        }
        return this.types[systemAttributeType];
    }

    public void Load()
    {
        while (this.loaders.Count>0)
        {
            long instanceId = this.loaders.Dequeue();
            Component component;
            if (true)
            {

            }
        }
    }

    public void Run<A, B, C>(string type, A a, B b, C c)
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

    public void Add(Component component)
    {
        
    }

    public void Remove(long instanceId)
    {
        this.allComponents.Remove(instanceId);
    }

    public Component Get(long instanceId)
    {
        Component component = null;
        this.allComponents.TryGetValue(instanceId, out component);
        return component;
    }

    public void Deserialize(Component component)
    {
        
    }

    public void Awake(Component component)
    {

    }





    #endregion

}

