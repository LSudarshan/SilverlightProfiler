using NMetrics.Filters;
using NMetrics.Visitors;

namespace NMetrics
{
    public class Run
    {
        private static void Main(string[] args)
        {
            Condition belongsToImdCondition = Condition.Eq("IMD").Or(Condition.Eq("IContact")).Or(Condition.Eq("BCG"));
//            var imd = new Project(@"E:\projects\BCG\IMD\src\IMDClient\IMDClient\Bin\Debug",belongsToImdCondition, "temp\\imd\\");
            var imd = new Project(@"E:\projects\BCG\IMD\src\IMDClient\IMDClient\Bin\Debug", belongsToImdCondition, "beforeModification\\imd\\");
            imd.Initialize();
            imd.Accept(new SilverlightProfilerVisitor(@"afterModification\\imd\\", "IMDClient", "ApplicationStartup", "IMDClient.App"));
        }
    }
}