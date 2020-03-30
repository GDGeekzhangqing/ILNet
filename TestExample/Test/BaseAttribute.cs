using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class BaseAttribute : Attribute
    {
    private string hello;

    public Type attributeType { get; }

    
    public BaseAttribute()
    {
        this.attributeType = this.GetType();
    }
}

