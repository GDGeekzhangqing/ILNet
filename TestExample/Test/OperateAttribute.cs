using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;




    /// <summary>
    /// 运算特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class OperateAttribute : Attribute
    {
        /// <summary>
        /// 运算特性
        /// </summary>
        /// <param name="operateNo">运算代号</param>
        public OperateAttribute(string operateNo)
        {
            this.operateNo = operateNo;
        }

        /// <summary>
        /// 运算代号
        /// </summary>
        string operateNo;
        /// <summary>
        /// 运算代号
        /// </summary>
        public string OperNo
        {
            get
            {
                return operateNo;
            }
        }

    }

