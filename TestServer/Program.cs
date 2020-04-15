using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ILNet.Mgr;
using Proltocol;

namespace TestServer
{
    class Program
    {
        static void Main(string[] args)
        {
            NetSocket<ServerSession, GameMsg> server = new NetSocket<ServerSession, GameMsg>();
            server.StartAsServer(IPCfg.srvIP, IPCfg.srvPort);

            Console.WriteLine("\nInput 'quit' to stop server!");
            while (true)
            {
                string ipt = Console.ReadLine();
                if (ipt == "quit")
                {
                    server.Close();
                    break;
                }
                if (ipt == "all")
                {
                    List<ServerSession> sessionLst = server.GetSesstionLst();
                    for (int i = 0; i < sessionLst.Count; i++)
                    {
                        sessionLst[i].SendMsg(new GameMsg
                        {
                            text = "broadcast from server."
                        });
                    }
                }
            }

        }
    }
}
