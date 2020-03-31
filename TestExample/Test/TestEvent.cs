using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Event(EventType.HelloWorld)]
public class TestEvent:AEvent<int,NumericType,int>
{
    public override void Run(int a, NumericType b, int c)
    {
        Console.WriteLine("Hellow World");
    }
}
