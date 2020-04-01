using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Event(EventType.HelloWorld)]
public class TestEvent:AEvent<Player,NumericType,int>
{
    public override void Run(Player a, NumericType b, int c)
    {
        Console.WriteLine("Hellow World");
    }
}
