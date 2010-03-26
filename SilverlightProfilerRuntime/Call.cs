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
        private string classWhichOwnsMethod;
        private DateTime enterTime;
        private double duration;

        public Call(string methodName, string classWhichOwnsMethod, Call parent)
        {
            this.methodName = methodName;
            this.classWhichOwnsMethod = classWhichOwnsMethod;
            this.parent = parent;
        }

        public void IncrementCount()
        {
            numberOfTimesCalledFromParent++;
        }

        public List<Call> Children
        {
            get { return children; }
        }

        public double Duration
        {
            get { return duration; }
        }

        public int NumberOfTimesCalledFromParent
        {
            get { return numberOfTimesCalledFromParent; }
        }

        public string FullName
        {
            get { return classWhichOwnsMethod + "." + methodName; }
        }

        public void Enter(DateTime time)
        {
            enterTime = time;
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
            duration += time.Subtract(enterTime).TotalMilliseconds;                      
        }

        public void Dump(StringBuilder stringBuilder)
        {
            stringBuilder.Append("\n");
            for (int i = 0; i < Depth(); i++)
            {
                stringBuilder.Append(" ");
            }
            stringBuilder.Append(FullName);
            children.ForEach(call => call.Dump(stringBuilder));
        }

        private int Depth()
        {
            if (parent == null) return 0;
            return parent.Depth() + 1;
        }
    }
}