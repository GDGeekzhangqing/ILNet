using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ILNet.Tools;


public class ServerRoot:ISingleton<ServerRoot>
{
    public int SeesionID = 0;

    public void Init()
    {
      
        NetSvc.Instance.Init();

        NetLogger.LogMsg("ServerRoot Init...");
    }


    public void Update()
    {
        NetSvc.Instance.Update();
    }

    public int GetSessionID()
    {
        if (SeesionID == int.MaxValue)
        {
            SeesionID = 0;
        }
        return SeesionID += 1;
    }


}

