using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ILNet.Mgr;
using Proltocol;

namespace TestClient
{
    class Program
    {
        static NetSocket<ClientSession,GameMsg> client;

        static void Main(string[] args)
        {
            client = new NetSocket<ClientSession, GameMsg>();
            client.StartAsClient(IPCfg.srvIP, IPCfg.srvPort);

            Console.WriteLine("\nInput 'quit' to stop client!");
            while (true)
            {
                string ipt = Console.ReadLine();
                if (ipt == "quit")
                {
                    client.Close();
                    break;
                }
                else
                {
                    client.session.SendMsg(new GameMsg
                    {
                        text = ipt
                    });
                }
            }
        }
    }
}
