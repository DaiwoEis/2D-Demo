using System;
using Lockstep.Math;
using System.Collections.Generic;

namespace Lockstep.Game
{

    [Serializable]
    public partial class CTimeLine : Component, ITimeLineHolder
    {
        [Backup] private List<TimeLine> timeLines = new List<TimeLine>();
        private List<string> activeTimeLines = new List<string>();
        [ReRefBackup] private Dictionary<string, Action<object[]>> callBackDic;

        public override void DoUpdate(LFloat deltaTime)
        {
            base.DoUpdate(deltaTime);

            foreach (var name in activeTimeLines)
                GetTimeLine(name)?.Update(deltaTime);

            for (int j = activeTimeLines.Count - 1; j >= 0; --j)
            {
                var timeLine = GetTimeLine(activeTimeLines[j]);
                if (timeLine != null && timeLine.End())
                    activeTimeLines.RemoveAt(j);
            }
        }

        private TimeLine GetTimeLine(string name)
        {
            return timeLines.Find(o => o.name == name);
        }

        public void Clear()
        {
            activeTimeLines.Clear();
            timeLines.Clear();
        }

        public void AddTimeLine(TimeLine timeLine)
        {
            if (GetTimeLine(timeLine.name) == null)
                timeLines.Add(timeLine);
            timeLine.SetHolder(this);
        }

        public void StartTimeLine(string name)
        {
            var timeLine = GetTimeLine(name);
            if (timeLine != null)
            {
                timeLine.Start();
                if (!activeTimeLines.Contains(timeLine.name))
                    activeTimeLines.Add(timeLine.name);
            }
        }

        public void SetCallBackDic(Dictionary<string, Action<object[]>> dic)
        {
            callBackDic = dic;
        }

        public void ReBindRef()
        {
            foreach (var timeLine in timeLines)
            {
                timeLine.SetHolder(this);
            }
        }

        public Action<object[]> GetCallBack(string name)
        {
            if (callBackDic != null && callBackDic.TryGetValue(name, out var callBack))
                return callBack;
            return null;
        }
    }

    public interface ITimeLineHolder
    {
        Action<object[]> GetCallBack(string name);
    }

    public partial class TimeLine : INeedBackup
    {
        public string name;
        public LFloat timer;
        public LFloat length;
        public List<TimeLineNode> nodes;

        [ReRefBackup] private ITimeLineHolder timeLineHolder;

        public void SetHolder(ITimeLineHolder holder)
        {
            timeLineHolder = holder;
        }

        public bool End()
        {
            return timer >= length;
        }

        public void Start()
        {
            timer = LFloat.zero;
        }

        public void Update(LFloat deltaTime)
        {
            var newTime = timer + deltaTime;
            foreach (var node in nodes)
            {
                if (node.time >= timer && node.time <= newTime)
                    timeLineHolder?.GetCallBack(node.callBackName)?.Invoke(node.parmas);
            }
            timer = newTime;
        }
    }

    public partial class TimeLineNode : INeedBackup
    {
        public LFloat time;
        [NoBackup] public object[] parmas;
        public string callBackName;
    }
}
