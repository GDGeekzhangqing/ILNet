﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ILNet.Net
{
    public class AckMgr<T, R> : IDisposable where R : AckMsg, new()
    {

        //主定时器
        public System.Timers.Timer Checktimer;
        public System.Timers.Timer Sendtimer;

        private readonly object lockObj = new object();


        //发送心跳包事件
        public event Action<T> SendHearEvent;

        //心跳包超时事件
        public event Action<T> ConnectLostEvent;

        //每个会话对应一个心跳KGHeartBeat
        private Dictionary<T, R> connectDic = new Dictionary<T, R>();

        public Dictionary<T, R> ConnectDic { get => connectDic; protected set => connectDic = value; }


        public virtual AckMgr<T, R> InitTimerEvent(Action<T> sendHearEvent, Action<T> connectLostEvent, double Checkinterval = 1000, double Sendinterval = 1000)
        {
            //这里是赋值每过多少秒执行一次事件
            Checktimer = new System.Timers.Timer(Checkinterval);
            Sendtimer = new System.Timers.Timer(Sendinterval);


            SendHearEvent = sendHearEvent;
            ConnectLostEvent = connectLostEvent;

            //定时执行事件
            Checktimer.Elapsed += (v, f) =>
            {
                //遍历每个会话回调一次检查心跳包
                CheckHeartBeat();
            };

            //定时执行事件
            Sendtimer.Elapsed += (v, f) =>
            {
                //遍历每个会话回调一次发送心跳包
                lock (lockObj)
                {
                    SendHeartBeat();
                }
            };

            return this;
        }

        /// <summary>
        /// 开始计时回调
        /// </summary>
        /// <returns></returns>
        public virtual AckMgr<T, R> StartTimer()
        {
            Checktimer.Start();
            Sendtimer.Start();
            return this;
        }

        /// <summary>
        /// 停止计时回调
        /// </summary>
        /// <returns></returns>
        public virtual AckMgr<T, R> StopTimer()
        {
            Checktimer.Stop();
            Sendtimer.Stop();
            return this;
        }


        /// <summary>
        /// 发送心跳包
        /// </summary>
        public virtual void SendHeartBeat()
        {
            lock (lockObj)
            {
                if (ConnectDic.Count > 0 && SendHearEvent != null)
                {
                    List<T> RemoveList = new List<T>();
                    foreach (KeyValuePair<T, R> item in ConnectDic)
                    {
                        SendHearEvent?.Invoke(item.Key);

                    }
                }
            }
        }

        /// <summary>
        /// 检查心跳包
        /// </summary>
        public virtual void CheckHeartBeat()
        {
            lock (lockObj)
            {
                if (ConnectDic.Count > 0)
                {
                    List<T> RemoveList = new List<T>();
                    foreach (KeyValuePair<T, R> item in ConnectDic)
                    {


                        //检查 心跳包超时 如果超时满次数就移除并回调事件
                        item.Value.CheckHeat();
                        if (item.Value.Lostcount >= item.Value.MaxLostcount)
                        {
                            RemoveList.Add(item.Key);

                        }
                        RemoveList.ForEach(remove =>
                        {
                            ConnectLostEvent(remove);
                            ConnectDic.Remove(remove);
                        });
                    }
                }
            }
        }

        /// <summary>
        /// 添加存储新的心跳包
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="heart"></param>
        /// <param name="maxlosttime"></param>
        /// <param name="maxlost"></param>
        public AckMgr<T, R> AddConnectDic(T obj, double maxlosttime = 2, int maxlost = 3)
        {
            R heartBeat = new R().InitMax<R>(maxlosttime, maxlost);

            lock (lockObj)
            {
                ConnectDic.Add(obj, heartBeat);
            }

            return this;
        }

        /// <summary>
        /// 移除连接
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public AckMgr<T, R> RemoveConnectDic(T obj)
        {
            lock (lockObj)
            {
                ConnectDic.Remove(obj);
            }

            return this;
        }

        /// <summary>
        /// 更新指定会话对应的心跳包
        /// </summary>
        /// <param name="obj"></param>
        public void UpdateOneHeat(T obj)
        {
            lock (lockObj)
            {
                ConnectDic[obj].UpdateHeat();
            }
        }

        /// <summary>
        /// 销毁时调用
        /// </summary>
        public void Dispose()
        {
            Checktimer.Dispose();
            Sendtimer.Dispose();
        }
    }
}
