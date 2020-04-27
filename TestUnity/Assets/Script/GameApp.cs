/****************************************************
    文件：GameApp.cs
	作者：GDGeek^掌情
    邮箱: 1286358939@qq.com
    日期：#CreateTime#
	功能：Nothing
*****************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class GameApp : MonoBehaviour
{

    #region 旧的

    public NetSvc netSvc = null;



    public void Start()
    {
        netSvc = GetComponent<NetSvc>();
        netSvc.InitSvc();

        Debug.Log("GameApp Start...");
    }



    #endregion



}