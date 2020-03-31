using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace TestEnum
{
    public class TEnum : IComparable<TEnum>, IEquatable<TEnum>
    {

        private static int counter = -1;
        private static Hashtable hashTable = new Hashtable();
        protected static List<TEnum> members = new List<TEnum>();
        private string Name { get; set; }
        private  int Value { get; set; }

        protected TEnum(string name)
        {
            this.Name = name;
            this.Value = ++counter;
            members.Add(this);
            if (!hashTable.ContainsKey(this.Value))
            {
                hashTable.Add(this.Value, this);
            }
        }


        public int CompareTo(TEnum other)
        {
            throw new NotImplementedException();
        }

        public bool Equals(TEnum other)
        {
            throw new NotImplementedException();
        }
    }
}
