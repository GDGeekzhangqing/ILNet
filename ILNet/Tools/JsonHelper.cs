using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace ILNet.Tools
{
    public class JsonHelper
    {
        /// <summary>
        /// 将对象序列化成Json格式
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static string SerializeObject(object o)
        {
            string json = "";
            try
            {
                json = JsonConvert.SerializeObject(o);
            }
            catch (Exception e)
            {
                NetLogger.LogMsg("无法应用这个方法");
            }

            return json;
        }

    }
}
