using ILNet.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TestExample
{


    class Program
    {
        public delegate void SaySomething(string name);
        public event SaySomething come;


        static void Main(string[] args)
        {
           /*  Console.WriteLine("调用Operations类内各函数：");
             UseOperation2(Operations.Add, 3, 5);
             UseOperation2(Operations.Minus, 9, 4);
             UseOperation2(Operations.Multiply, 3, 7);
             UseOperation2(Operations.Divide, 8, 2);
             UseOperation2(Operations.Modulo, 15, 4);*/

            /* Console.WriteLine("打印Operations类内各函数属性：");
             PrintOperations();*/

           /* Console.WriteLine("调用Operations类内各函数：");
            UseOperation("ADD", 3, 5);
            UseOperation("MINUS", 9, 4);
            UseOperation("MULTIPLY", 3, 7);
            UseOperation("DIVIDE", 8, 2);
            UseOperation("MOD", 15, 4);*/

            /* Program program = new Program();

             program.Test();*/

            


            Console.ReadKey();
        }

        #region 委托事件
        public void Test() { SaySomething sayhello = new SaySomething(SayHello); SaySomething saynice = new SaySomething(SayNiceToMeetYou); come += sayhello; come += saynice; come("张三"); }
        public void SayHello(string name) { Console.WriteLine("Hello," + name + "!"); }

        public void SayNiceToMeetYou(string name) { Console.WriteLine("Nice to meet you," + name + "!"); }

        #endregion

        #region 特性
        public delegate TOutput OperDelegate<TInput, TOutput>(TInput input1, TInput input2);

        /// <summary>
        /// 调用Operations类内各函数
        /// </summary>
        /// <param name="operNo">运算代号</param>
        /// <param name="a">运算符a</param>
        /// <param name="b">运算符b</param>
        public static void UseOperation(string operNo, int a, int b,Type type)
        {
           
            foreach (MethodInfo mInfo in (new OP()).GetType().GetMethods())
            {
                foreach (Attribute attr in Attribute.GetCustomAttributes(mInfo))
                {
                    if (attr is OperateAttribute &&
                        (attr as OperateAttribute).OperNo == operNo)
                    {
                        Console.WriteLine(string.Format("{0} {1} {2} = {3}",
                            a, operNo, b, mInfo.Invoke(null, new object[] { a, b })));
                    }
                }
            }
        }

        public static void UseOperation2<TInput, TOutput>(
            OperDelegate<TInput, TOutput> operDelegate, TInput input1, TInput input2)
        {
            TOutput output = operDelegate(input1, input2);
            Console.WriteLine(string.Format("{0} {1} {2} = {3}",
                input1, operDelegate.Method.Name, input2, output));
        }

        /// <summary>
        /// 打印Operations类内各函数属性
        /// </summary>
        public static void PrintOperations()
        {
            foreach (MethodInfo mInfo in (new Operations()).GetType().GetMethods())
            {
                foreach (Attribute attr in Attribute.GetCustomAttributes(mInfo))
                {
                    if (attr is OperateAttribute)
                    {
                        string operNo = (attr as OperateAttribute).OperNo;
                        Console.WriteLine("=====================");
                        Console.WriteLine("关联属性：" + mInfo.Attributes);
                        Console.WriteLine("函数名称：" + mInfo.Name);
                        Console.WriteLine("成员类型：" + mInfo.MemberType);
                        Console.WriteLine("参数0类型：" + mInfo.GetParameters()[0].ParameterType.ToString());
                        Console.WriteLine("参数1类型：" + mInfo.GetParameters()[1].ParameterType.ToString());
                        Console.WriteLine("返回值类型：" + mInfo.ReturnParameter.ParameterType.ToString());
                        
                       

                    }
                }
            }
        }
        #endregion
    }
}
