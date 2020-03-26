﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ILNet.ILNetHelper
{
    public class ISingleton<T> where T : new() 
    {
        private static  T instance;

        public static T Instance
        {
            get
            {
                if (instance==null)
                {
                    instance = new T();
                }
                return instance;
            }
        
        }
       

    }
}
