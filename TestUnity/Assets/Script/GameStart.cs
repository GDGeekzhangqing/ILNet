/****************************************************
    文件：GameStart.cs
	作者：GDGeek^掌情
    邮箱: 1286358939@qq.com
    日期：#CreateTime#
	功能：Nothing
*****************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ILNet.Tools;
using ILNet.Mgr;
using Proltocol;

public class GameStart : MonoBehaviour 
{
   NetSocket<ClientSession, GameMsg> skt = null;

    private void Start()
    {
        skt = new NetSocket<ClientSession, GameMsg>();
        skt.StartAsClient(IPCfg.srvIP, IPCfg.srvPort);

        skt.SetLog(true, (string msg, int lv) => {
            switch (lv)
            {
                case 0:
                    msg = "Log:" + msg;
                    Debug.Log(msg);
                    break;
                case 1:
                    msg = "Warn:" + msg;
                    Debug.LogWarning(msg);
                    break;
                case 2:
                    msg = "Error:" + msg;
                    Debug.LogError(msg);
                    break;
                case 3:
                    msg = "Info:" + msg;
                    Debug.Log(msg);
                    break;
            }
        });
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            skt.session.SendMsg(new GameMsg
            {
                text = "Hello Unity"
            });
        }
    }
}