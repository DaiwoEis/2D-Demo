using System;
using Lockstep.Math;
using System.Collections.Generic;

namespace Lockstep.Game
{

    [Serializable]
    public partial class CTimeLine : Component
    {
        private Dictionary<string, TimeLine> timeLines = new Dictionary<string, TimeLine>();
        private List<TimeLine> activeTimeLines = new List<TimeLine>();

        public override void DoUpdate(LFloat deltaTime)
        {
            base.DoUpdate(deltaTime);

            foreach (var timeLine in activeTimeLines)
                timeLine.Update(deltaTime);

            for (int j = activeTimeLines.Count - 1; j >= 0; --j)
            {
                if (activeTimeLines[j].end)
                    activeTimeLines.RemoveAt(j);
            }
        }

        public void Clear()
        {
            activeTimeLines.Clear();
            timeLines.Clear();
        }

        public void AddTimeLine(TimeLine timeLine)
        {
            if (!timeLines.ContainsKey(timeLine.name))
                timeLines.Add(timeLine.name, timeLine);
            else
                timeLines[timeLine.name] = timeLine;
        }

        public void StartTimeLine(string name)
        {
            if (timeLines.ContainsKey(name))
            {
                var timeLine = timeLines[name];
                timeLine.Start();
                if (!activeTimeLines.Contains(timeLine))
                    activeTimeLines.Add(timeLine);
            }
        }
    }

    public class TimeLine
    {
        public string name;
        public LFloat timer;
        public LFloat length;
        public List<TimeLineNode> nodes;

        public bool end => timer >= length;

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
                    node.callBack?.Invoke(node.parmas);
            }
            timer = newTime;
        }
    }

    public class TimeLineNode
    {
        public LFloat time;
        public object[] parmas;
        public Action<object[]> callBack;
    }
}
