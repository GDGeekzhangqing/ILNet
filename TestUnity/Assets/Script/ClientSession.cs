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
using ILNet.Net;
using ILNet.Tools;
using Proto;

public class ClientSession : ISession<GameMsg>
{

    protected override void OnDisConnected()
    {
        base.OnDisConnected();

        NetLogger.LogMsg("下线");
    }


}