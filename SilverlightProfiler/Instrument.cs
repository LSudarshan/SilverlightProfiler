using System;
using NMetrics;
using NMetrics.Filters;
using SilverlightProfilerRuntime;

namespace SilverlightProfiler
{
    public class Instrument
    {
        private static void Main(string[] args)
        {
            if(args.Length != 3)
            {
                Console.WriteLine("Usage is : Instrument mainSilverlightAssembly silverlightStartupType methodToAddProfilingHook");
                return;
            }
            string mainSilverlightAssembly = args[0];
            string silverlightStartupType = args[1];
            string methodToAddProfilingHook = args[2];

            Condition belongsToImdCondition = Condition.Eq("IMD").Or(Condition.Eq("IContact")).Or(Condition.Eq("BCG"));
            var imd = new Project(@"E:\projects\NMetrics\Temp", belongsToImdCondition,
                                  "beforeModification\\");
            imd.Initialize();
            imd.Accept(new SilverlightProfilerVisitor(@"afterModification\\", mainSilverlightAssembly,
                                                      silverlightStartupType, methodToAddProfilingHook));
        }
    }
}