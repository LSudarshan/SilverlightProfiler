using NMetrics.Filters;
using NMetrics.Visitors;

namespace NMetrics
{
    public class Run
    {
        private static void Main(string[] args)
        {
            Condition belongsToImdCondition = Condition.Eq("IMD").Or(Condition.Eq("IContact")).Or(Condition.Eq("BCG"));
            var imd = new Project(@"E:\projects\BCG\IMD\src", "imd",
                                  Condition.Eq("silverlight").And(belongsToImdCondition));
            imd.Initialize();
            imd.Accept(new SilverlightProfilerVisitor());
        }
    }
}