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
using Proltocol;

public class ClientSession : NetSession<GameMsg>
{
    protected override void OnConnected()
    {
    }

    protected override void OnReciveMsg(GameMsg msg)
    {
        NetLogger.LogMsg("Server Response:" + msg.text);
    }

    protected override void OnDisConnected()
    {
    }
}