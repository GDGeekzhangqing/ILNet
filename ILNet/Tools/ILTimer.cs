﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace ILNet.Tools
{
    public enum ILTimerUnit
    {
        Millisecond,
        Second,
        Mintue,
        Hour,
        Day
    }

    public class ILTimer
    {
        private Action<string> taskLog;
        private Action<Action<int>, int> taskHandle;
        private static readonly string lockTid = "lockTid";
        private DateTime startDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        private double nowTime;
        private Timer srvTimer;
        private int tid;
        private List<int> tidLst = new List<int>();
        private List<int> recTidLst = new List<int>();


        private static readonly string lockTime = "lockTime";
        private List<ILTimerTask> tmpTimeLst = new List<ILTimerTask>();
        private List<ILTimerTask> taskTimeLst = new List<ILTimerTask>();
        private List<int> tmpDelTimeLst = new List<int>();

        private int frameCounter;

        private static readonly string lockFrame = "lockFrame";
        private List<ILFrameTask> tmpFrameLst = new List<ILFrameTask>();
        private List<ILFrameTask> taskFrameLst = new List<ILFrameTask>();
        private List<int> tmpDelFrameLst = new List<int>();


        public ILTimer(int interval = 0)
        {
            tidLst.Clear();
            recTidLst.Clear();

            tmpTimeLst.Clear();
            taskTimeLst.Clear();

            tmpTimeLst.Clear();
            taskTimeLst.Clear();

            if (interval != 0)
            {
                srvTimer = new Timer(interval)
                {
                    AutoReset = true
                };

                srvTimer.Elapsed += (object sender, ElapsedEventArgs args) =>
                  {
                      Update();
                  };

                srvTimer.Start();
            }
        }

        public void Update()
        {
            CheckTimeTask();
            CheckFrameTask();

            DelTimeTask();
            DelFrameTask();

            if (recTidLst.Count>0)
            {
                lock (lockTid)
                {
                    RecycleTid();
                }
            }
        }
        /// <summary>
        /// 删除时间任务
        /// </summary>
        private void DelTimeTask()
        {
            if (tmpDelTimeLst.Count > 0)
            {
                lock (lockTime)
                {
                    for (int i = 0; i < tmpDelTimeLst.Count; i++)
                    {
                        bool isDel = false;
                        int delTid = tmpDelTimeLst[i];
                        for (int j = 0; j < taskTimeLst.Count; j++)
                        {
                            ILTimerTask task = taskTimeLst[j];
                            if (task.tid == delTid)
                            {
                                isDel = true;
                                taskTimeLst.RemoveAt(j);

                                break;
                            }
                        }

                        if (isDel)
                            continue;

                        for (int j = 0; j < tmpTimeLst.Count; j++)
                        {
                            ILTimerTask task = tmpTimeLst[j];
                            if (task.tid == delTid)
                            {
                                tmpTimeLst.RemoveAt(j);
                                recTidLst.Add(delTid);

                                break;
                            }
                        }

                    }
                }
            }
        }
        /// <summary>
        /// 删除帧任务
        /// </summary>
        private void DelFrameTask()
        {
            if (tmpDelFrameLst.Count > 0)
            {
                lock (lockFrame)
                {
                    for (int i = 0; i < tmpDelFrameLst.Count; i++)
                    {
                        bool isDel = false;
                        int delTid = tmpDelFrameLst[i];
                        for (int j = 0; j < taskFrameLst.Count; j++)
                        {
                            ILFrameTask task = taskFrameLst[j];
                            if (task.tid == delTid)
                            {
                                isDel = true;
                                taskFrameLst.RemoveAt(j);
                                recTidLst.Add(delTid);
                                break;
                            }
                        }

                        if (isDel)
                            continue;
                        for (int j = 0; j < tmpFrameLst.Count; j++)
                        {
                            ILFrameTask task = tmpFrameLst[j];
                            if (task.tid == delTid)
                            {
                                tmpFrameLst.RemoveAt(j);
                                recTidLst.Add(delTid);
                                break;
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 检查当前时间任务
        /// </summary>
        private void CheckTimeTask()
        {
            if (tmpTimeLst.Count > 0)
            {
                lock (lockTime)
                {
                    //加入缓存区中的定时任务
                    for (int tmpIndex = 0; tmpIndex < tmpTimeLst.Count; tmpIndex++)
                    {
                        taskTimeLst.Add(tmpTimeLst[tmpIndex]);
                    }

                    tmpTimeLst.Clear();
                }
            }

            //遍历检测任务是否达到条件
            nowTime = GetMillisecondsTime();
            for (int index = 0; index < taskTimeLst.Count; index++)
            {
                ILTimerTask task = taskTimeLst[index];
                if (nowTime.CompareTo(task.destTime) < 0)
                {
                    continue;
                }
                else
                {
                    Action<int> cb = task.callback;
                    try
                    {
                        if (taskHandle != null)
                        {
                            taskHandle(cb, task.tid);
                        }
                        else
                        {
                            if (cb != null)
                            {
                                cb(task.tid);
                            }
                        }
                    }
                    catch (Exception e)
                    {

                        LogInfo(e.ToString());

                    }

                    //移除已经完成的任务
                    if (task.count == 1)
                    {
                        taskTimeLst.RemoveAt(index);
                        index--;
                        recTidLst.Add(task.tid);
                    }
                    else
                    {
                        if (task.count != 0)
                        {
                            task.count -= 1;
                        }
                        task.destTime += task.delay;
                    }
                }
            }
        }

        /// <summary>
        /// 检查当前帧任务
        /// </summary>
        private void CheckFrameTask()
        {
            if (tmpFrameLst.Count > 0)
            {
                lock (lockFrame)
                {
                    //加入缓存区的定时任务
                    for (int tmpIndex = 0; tmpIndex < tmpFrameLst.Count; tmpIndex++)
                    {
                        taskFrameLst.Add(taskFrameLst[tmpIndex]);
                    }
                    tmpFrameLst.Clear();
                }
            }

            frameCounter += 1;
            //遍历检测任务是否达到条件
            for (int index = 0; index < taskFrameLst.Count; index++)
            {
                ILFrameTask task = taskFrameLst[index];
                if (frameCounter < task.destFrame)
                {
                    continue;
                }
                else
                {
                    Action<int> cb = task.callback;
                    try
                    {
                        if (taskHandle != null)
                        {
                            taskHandle(cb, task.tid);
                        }
                        else
                        {
                            if (cb != null)
                            {
                                cb(task.tid);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        LogInfo(e.ToString());
                    }

                    //移除已经完成的任务
                    if (task.count == 1)
                    {
                        taskFrameLst.RemoveAt(index);
                        index--;
                        recTidLst.Add(task.tid);
                    }
                    else
                    {
                        if (task.count != 0)
                        {
                            task.count = -1;
                        }
                        task.destFrame += task.delay;

                    }

                }
            }


        }

        #region TimeTask
        /// <summary>
        /// 添加定时任务
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="delay"></param>
        /// <param name="timerUnit"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public int AddTimeTask(Action<int> callback, double delay, ILTimerUnit timerUnit = ILTimerUnit.Millisecond, int count = 1)
        {
            if (timerUnit != ILTimerUnit.Millisecond)
            {
                switch (timerUnit)
                {

                    case ILTimerUnit.Second:
                        delay = delay * 1000;
                        break;
                    case ILTimerUnit.Mintue:
                        delay = delay * 1000 * 60;
                        break;
                    case ILTimerUnit.Hour:
                        delay = delay * 1000 * 60 * 60;
                        break;
                    case ILTimerUnit.Day:
                        delay = delay * 1000 * 60 * 60 * 24;
                        break;
                    default:
                        LogInfo("Add Task TimeUnit Type Error...");
                        break;
                }
            }

            int tid = GetTid();
            nowTime = GetUTCMilliseconds();
            lock (lockTime)
            {
                tmpTimeLst.Add(new ILTimerTask(tid, callback, nowTime + delay, delay, count));
            }

            return tid;
        }

        public void DeleteTimeTask(int tid)
        {
            lock (lockTime)
            {
                tmpDelTimeLst.Add(tid);
                //LogInfo("TmpDel ID:" + System.Threading.Thread.CurrentThread.ManagedThreadId.ToString());
            }
            /*
             bool exist = false;

             for (int i = 0; i < taskTimeLst.Count; i++) {
                 ILTimeTask task = taskTimeLst[i];
                 if (task.tid == tid) {
                     //taskTimeLst.RemoveAt(i);
                     for (int j = 0; j < tidLst.Count; j++) {
                         if (tidLst[j] == tid) {
                             //tidLst.RemoveAt(j);
                             break;
                         }
                     }
                     exist = true;
                     break;
                 }
             }

             if (!exist) {
                 for (int i = 0; i < tmpTimeLst.Count; i++) {
                     ILTimeTask task = tmpTimeLst[i];
                     if (task.tid == tid) {
                         //tmpTimeLst.RemoveAt(i);
                         for (int j = 0; j < tidLst.Count; j++) {
                             if (tidLst[j] == tid) {
                                 //tidLst.RemoveAt(j);
                                 break;
                             }
                         }
                         exist = true;
                         break;
                     }
                 }
             }

             return exist;
             */
        }


        public bool ReplaceTimeTask(int tid, Action<int> callback, float delay, ILTimerUnit timerUnit = ILTimerUnit.Millisecond, int count = 1)
        {
            if (timerUnit != ILTimerUnit.Millisecond)
            {
                switch (timerUnit)
                {

                    case ILTimerUnit.Second:
                        delay = delay * 1000;
                        break;
                    case ILTimerUnit.Mintue:
                        delay = delay * 1000 * 60;
                        break;
                    case ILTimerUnit.Hour:
                        delay = delay * 1000 * 60 * 60;
                        break;
                    case ILTimerUnit.Day:
                        delay = delay * 1000 * 60 * 60 * 24;
                        break;
                    default:
                        LogInfo("Replace Task TimeUnit Type Error...");
                        break;
                }
            }

            nowTime = GetUTCMilliseconds();
            ILTimerTask newTask = new ILTimerTask(tid, callback, nowTime + delay, delay, count);

            bool isRep = false;
            for (int i = 0; i < taskTimeLst.Count; i++)
            {
                if (taskTimeLst[i].tid == tid)
                {
                    taskTimeLst[i] = newTask;
                    isRep = true;
                    break;
                }
            }

            if (!isRep)
            {
                for (int i = 0; i < tmpTimeLst.Count; i++)
                {
                    if (tmpTimeLst[i].tid == tid)
                    {
                        tmpTimeLst[i] = newTask;
                        break;
                    }
                }
            }

            return isRep;
        }

        #endregion

        #region FrameTask
        /// <summary>
        /// 添加帧任务
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="delay"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public int AddFrameTask(Action<int> callback, int delay, int count = 1)
        {
            int tid = GetTid();
            lock (lockTime)
            {
                tmpFrameLst.Add(new ILFrameTask(tid, callback, frameCounter + delay, delay, count));
            }
            return tid;
        }
        /// <summary>
        /// 删除帧任务
        /// </summary>
        /// <param name="tid"></param>
        public void DeleteFrameTask(int tid)
        {
            lock (lockFrame)
            {
                tmpDelFrameLst.Add(tid);
            }
            /*
            bool exist = false;

            for (int i = 0; i < taskFrameLst.Count; i++) {
                PEFrameTask task = taskFrameLst[i];
                if (task.tid == tid) {
                    //taskFrameLst.RemoveAt(i);
                    for (int j = 0; j < tidLst.Count; j++) {
                        if (tidLst[j] == tid) {
                            //tidLst.RemoveAt(j);
                            break;
                        }
                    }
                    exist = true;
                    break;
                }
            }

            if (!exist) {
                for (int i = 0; i < tmpFrameLst.Count; i++) {
                    PEFrameTask task = tmpFrameLst[i];
                    if (task.tid == tid) {
                        //tmpFrameLst.RemoveAt(i);
                        for (int j = 0; j < tidLst.Count; j++) {
                            if (tidLst[j] == tid) {
                                //tidLst.RemoveAt(j);
                                break;
                            }
                        }
                        exist = true;
                        break;
                    }
                }
            }

            return exist;
            */
        }

        public bool ReplaceFrameTask(int tid, Action<int> callback, int delay, int count = 1)
        {
            ILFrameTask newTask = new ILFrameTask(tid, callback, frameCounter + delay, delay, count);

            bool isRep = false;
            for (int i = 0; i < taskFrameLst.Count; i++)
            {
                if (taskFrameLst[i].tid == tid)
                {
                    taskFrameLst[i] = newTask;
                    isRep = true;
                    break;
                }
            }

            if (!isRep)
            {
                for (int i = 0; i < tmpFrameLst.Count; i++)
                {
                    if (tmpFrameLst[i].tid == tid)
                    {
                        tmpFrameLst[i] = newTask;
                        isRep = true;
                        break;
                    }
                }
            }

            return isRep;
        }

        #endregion

        /// <summary>
        /// 设置日志
        /// </summary>
        /// <param name="log"></param>
        public void SetLog(Action<string> log)
        {
            taskLog = log;
        }
        /// <summary>
        /// 设置监听
        /// </summary>
        /// <param name="handle"></param>
        public void SetHandle(Action<Action<int>, int> handle)
        {
            taskHandle = handle;
        }
        /// <summary>
        /// 重置
        /// </summary>
        public void Reset()
        {
            tid = 0;
            tidLst.Clear();
            recTidLst.Clear();

            tmpTimeLst.Clear();
            taskFrameLst.Clear();

            taskLog = null;
            srvTimer.Stop();
        }
        /// <summary>
        /// 获取当前哪年
        /// </summary>
        /// <returns></returns>
        public int GetYear()
        {
            return GetLocalDateTime().Year;
        }
        /// <summary>
        /// 获取当前月份
        /// </summary>
        /// <returns></returns>
        public int GetMonth()
        {
            return GetLocalDateTime().Month;
        }
        /// <summary>
        /// 获取当前月份哪一天
        /// </summary>
        /// <returns></returns>
        public  int GetDay()
        {
            return GetLocalDateTime().Day;
        }
        /// <summary>
        /// 获取当前星期几
        /// </summary>
        /// <returns></returns>
        public  int GetWeek()
        {
            return (int)GetLocalDateTime().DayOfWeek;
        } 
        /// <summary>
        /// 获取当地时间
        /// </summary>
        /// <returns></returns>
        public  DateTime GetLocalDateTime()
        {
            DateTime dt = TimeZone.CurrentTimeZone.ToLocalTime(startDateTime.AddMilliseconds(nowTime));
            return dt;
        }
        public double GetMillisecondsTime()
        {
            return nowTime;
        }
        /// <summary>
        /// 获取当地时间字符串
        /// </summary>
        /// <returns></returns>
        public string GetLocalTimeStr()
        {
            DateTime dt = GetLocalDateTime();
            string str = GetTimeStr(dt.Hour) + ":" + GetTimeStr(dt.Minute) + ":" + GetTimeStr(dt.Second);
            return str;

        }

        /// <summary>
        /// 时间戳转为C#格式时间
        /// </summary>
        /// <param name="timeStamp">Unix时间戳格式</param>
        /// <returns>C#格式时间</returns>
        public static DateTime GetStampDateTime(string timeStamp)
        {
            DateTime time = new DateTime();
            try
            {
                DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
                long lTime = long.Parse(timeStamp + "0000000");
                TimeSpan toNow = new TimeSpan(lTime);
                time = dtStart.Add(toNow);
            }
            catch
            {
                time = DateTime.Now.AddDays(-30);
            }
            return time;
        }

        #region Tool Methonds
        /// <summary>
        /// 获取任务ID
        /// </summary>
        /// <returns></returns>
        private int GetTid()
        {
            lock (lockTid)
            {
                tid += 1;

                //安全代码，以防万一
                while (true)
                {
                    if (tid==int.MaxValue)
                    {
                        tid = 0;
                    }

                    bool used = false;

                    for (int i = 0; i < tidLst.Count; i++)
                    {
                        if (tid==tidLst[i])
                        {
                            used = true;
                            break;
                        }
                    }
                    if (!used)
                    {
                        tidLst.Add(tid);
                        break;
                    }
                    else
                    {
                        tid += 1;
                    }
                }
            }

            return tid;
        }

        private void RecycleTid()
        {
            for (int i = 0; i < recTidLst.Count; i++)
            {
                int tid = recTidLst[i];

                for (int j = 0; j < tidLst.Count; j++)
                {
                    if (tidLst[j]==tid)
                    {
                        tidLst.RemoveAt(j);
                        break;
                    }
                }
            }

            recTidLst.Clear();
        }


        private void LogInfo(string info)
        {
            if (taskLog!=null)
            {
                taskLog(info);
            }
        }

        private double GetUTCMilliseconds()
        {
            TimeSpan ts = DateTime.UtcNow - startDateTime;
            return ts.TotalMilliseconds;
        }
        private string GetTimeStr(int time)
        {
            if (time < 10)
            {
                return "0" + time;
            }
            else
            {
                return time.ToString();
            }
        }

        #endregion
    }

    partial class ILTimerTask
    {
        public int tid;
        public Action<int> callback;
        public double destTime;
        public double delay;
        public int count;

        public ILTimerTask(int tid, Action<int> callback, double destTime, double delay, int count)
        {
            this.tid = tid;
            this.callback = callback;
            this.destTime = destTime;
            this.delay = delay;
            this.count = count;
        }
    }

    partial class ILFrameTask 
    {
        public int tid;
        public Action<int> callback;
        public int destFrame;
        public int delay;
        public int count;

        public ILFrameTask(int tid, Action<int> callback, int destFrame, int delay, int count)
        {
            this.tid = tid;
            this.callback = callback;
            this.destFrame = destFrame;
            this.delay = delay;
            this.count = count;
        }
    }


}
