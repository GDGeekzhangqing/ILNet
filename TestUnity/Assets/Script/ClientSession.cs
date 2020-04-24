/****************************************************
    文件：ClientSession.cs
	作者：GDGeek^掌情
    邮箱: 1286358939@qq.com
    日期：#CreateTime#
	功能：Nothing
*****************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ILNet.Mgr;
using ILNet.Tools;
using Proto;

public class ClientSession : ISession<GameMsg>
{

   

    protected override void OnReciveMsg(GameMsg msg)
    {
        NetSvc.Instance.AddNetPkg(msg);
        NetLogger.LogMsg($"服务端回应:{msg.chatMsg}");
    }

    protected override void OnDisConnected()
    {
        base.OnDisConnected();
        isOffLine = true;
        NetLogger.LogMsg("下线");

    }
}