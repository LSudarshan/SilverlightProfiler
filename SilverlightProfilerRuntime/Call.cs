using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SilverlightProfilerRuntime
{
    public class Call
    {
        private List<Call> children = new List<Call>();
        private Call parent;
        private int numberOfTimesCalledFromParent;
        private string methodName;
        private DateTime enterTime;
        private double duration;
        private double profilerMethodsExecutionDuration;

        public Call(string methodName, Call parent)
        {
            this.methodName = methodName;
            this.parent = parent;
        }

        public void IncrementCount()
        {
            numberOfTimesCalledFromParent++;
        }

        public List<Call> Children
        {
            get
            {
                return children;
            }
        }

        public List<Call> SortedChildren
        {
            get
            {
                List<Call> calls = new List<Call>(Children);
                calls.Sort(Comparison);
                return calls;
            }
        }

        private int Comparison(Call x, Call y)
        {
            return y.Duration.CompareTo(x.Duration);
        }

        public double Duration
        {
            get { return duration - ProfilerMethodsExecutionDuration; }
        }

        protected double ProfilerMethodsExecutionDuration
        {
            get
            {
                double total = 0;
                children.ForEach(call => total += call.ProfilerMethodsExecutionDuration);
                return profilerMethodsExecutionDuration + total;
            }
        }

        public int NumberOfTimesCalledFromParent
        {
            get { return numberOfTimesCalledFromParent; }
        }

        public virtual string FullName
        {
            get
            {
                return methodName;
            }
        }

        public virtual bool IsThreadRoot
        {
            get { return false; }
        }

        public void Enter(DateTime time)
        {
            enterTime = time;
            profilerMethodsExecutionDuration += DateTime.Now.Subtract(enterTime).TotalMilliseconds;
        }

        public bool HasChild(Call call)
        {
            return GetChild(call) != null;
        }

        public Call GetChild(Call call)
        {
            return Children.FirstOrDefault(call1 => call1.FullName == call.FullName);
        }

        public void Exit(DateTime time)
        {
            profilerMethodsExecutionDuration += DateTime.Now.Subtract(time).TotalMilliseconds;
            duration += time.Subtract(enterTime).TotalMilliseconds;
        }

        public void Dump(StringBuilder stringBuilder)
        {
            stringBuilder.Append("\n");
            for (int i = 0; i < Depth(); i++)
            {
                stringBuilder.Append(" ");
            }
            stringBuilder.Append(FullName + "  " + NumberOfTimesCalledFromParent + "  " + Duration);
            children.ForEach(call => call.Dump(stringBuilder));
        }

        private int Depth()
        {
            if (parent == null) return 0;
            return parent.Depth() + 1;
        }

    }
}