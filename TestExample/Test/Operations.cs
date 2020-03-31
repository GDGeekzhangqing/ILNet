using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


   public  class Operations
    {
        [Operate("ADD")]
        public static int Add(int a, int b) { return a + b; }

        [Operate("MINUS")]
        public static int Minus(int a, int b) { return a - b; }

        [Operate("MULTIPLY")]
        public static int Multiply(int a, int b) { return a * b; }

        [Operate("DIVIDE")]
        public static int Divide(int a, int b) { return a / b; }

        [Operate("MOD")]
        public static int Modulo(int a, int b) { return a % b; }

    }

