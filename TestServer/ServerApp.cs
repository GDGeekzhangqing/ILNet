using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ILNet.Mgr;
using Proto;
using ILNet.Net;
using ILNet.Tools;
using System.Threading;

namespace TestServer
{
    class ServerApp
    {
        static void Main(string[] args)
        {

            ServerRoot.Instance.Init();
            Console.WriteLine("Hello World!");

            while (true)
            {
                ServerRoot.Instance.Update();
                //Thread.Sleep(20);
            }

        }
    }
}
