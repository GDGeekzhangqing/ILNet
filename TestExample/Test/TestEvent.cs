using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Event(EventIdType.NumbericChange)]
public class TestEvent:AEvent<long,Player,int>
{
    public override void Run(long a, Player b, int c)
    {
        Console.WriteLine("Hellow World");
    }
}
