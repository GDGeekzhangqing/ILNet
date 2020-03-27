using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


[Base(EventType.Hello)]
public class TestEvent
{
    public void HelloWorld()
    {
        Console.WriteLine("Hellow World");
    }
}
