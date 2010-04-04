using System;
using System.Diagnostics;
using NMetrics;
using NMetrics.Filters;
using SilverlightProfilerRuntime;
using SilverlightProfilerUnitTest;

namespace SilverlightProfiler
{
    public class Instrument
    {
        private static void Main(string[] args)
        {
            if(args.Length != 5)
            {
                Console.WriteLine("Usage is : Instrument mainSilverlightAssembly silverlightStartupType methodToAddProfilingHook originalPathOfDlls assemblyPatternsToInstrument");
                return;
            }
            
            string mainSilverlightAssembly = args[0];
            string silverlightStartupType = args[1];
            string methodToAddProfilingHook = args[2];
            string originalPathOfDlls = args[3];
            string assemblyPatternsToInstrument = args[4];

            Condition belongsToImdCondition = new AssembliesPatternParser().Parse(assemblyPatternsToInstrument);
            var imd = new Project(originalPathOfDlls, belongsToImdCondition, originalPathOfDlls);
            imd.Initialize();
            imd.Accept(new SilverlightProfilerVisitor(originalPathOfDlls, mainSilverlightAssembly, silverlightStartupType, methodToAddProfilingHook));
        }
    }
}