using System;
using Lockstep.Math;
using System.Collections.Generic;

namespace Lockstep.Game
{

    [Serializable]
    public partial class CTimeLine : Component, ITimeLineHolder
    {
        [Backup] private List<TimeLine> timeLines = new List<TimeLine>();
        private List<string> endTimeLines = new List<string>();
        [ReRefBackup] private Dictionary<string, Action<object[]>> callBackDic;

        public override void DoUpdate(LFloat deltaTime)
        {
            base.DoUpdate(deltaTime);

            endTimeLines.Clear();
            foreach (var timeLine in timeLines)
            {
                if (timeLine != null && timeLine.IsStart())
                {
                    timeLine.Update(deltaTime);
                    if (timeLine.IsEnd())
                        endTimeLines.Add(timeLine.name);
                }
            }
            for (int j = endTimeLines.Count - 1; j >= 0; --j)
            {
                var timeLine = GetTimeLine(endTimeLines[j]);
                timeLine?.End();
            }
            endTimeLines.Clear();
        }

        private TimeLine GetTimeLine(string name)
        {
            return timeLines.Find(o => o.name == name);
        }

        public void Clear()
        {
            endTimeLines.Clear();
            timeLines.Clear();
        }

        public void AddTimeLine(TimeLine timeLine)
        {
            if (GetTimeLine(timeLine.name) == null)
                timeLines.Add(timeLine);
        }

        public void AddNode(string name, TimeLineNode node)
        {
            var timeLine = GetTimeLine(name);
            if (timeLine != null)
                timeLine.nodes.Add(node);
        }

        public void StartTimeLine(string name)
        {
            var timeLine = GetTimeLine(name);
            if (timeLine != null)
            {
                timeLine.Start();
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

    [Serializable]
    public partial class TimeLine : INeedBackup
    {
        public string name;
        public LFloat timer;
        public LFloat length;
        public bool start;
        [NoBackup] public List<TimeLineNode> nodes = new List<TimeLineNode>();
        [ReRefBackup] private ITimeLineHolder timeLineHolder;

        public void SetHolder(ITimeLineHolder holder)
        {
            timeLineHolder = holder;
        }

        public bool IsEnd()
        {
            return timer >= length;
        }

        public void End()
        {
            start = false; 
        }

        public bool IsStart()
        {
            return start;
        }

        public void Start()
        {
            timer = LFloat.zero;
            start = true;
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

    public partial class TimeLineNode
    {
        public LFloat time;
        public object[] parmas;
        public string callBackName;
    }
}
