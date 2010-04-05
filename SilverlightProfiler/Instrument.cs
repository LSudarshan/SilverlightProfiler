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
            if(args.Length != 6)
            {
                Console.WriteLine("Usage is : Instrument mainSilverlightAssembly silverlightStartupType methodToAddProfilingHook originalPathOfDlls assemblyPatternsToInstrument ClassPatternsToInstrument");
                return;
            }
            
            string mainSilverlightAssembly = args[0];
            string silverlightStartupType = args[1];
            string methodToAddProfilingHook = args[2];
            string originalPathOfDlls = args[3];
            string assemblyPatternsToInstrument = args[4];
            string typePatternsToInstrument = args[5];

            Condition assemblyInstrumentCondition = new ConditionPatternParser().Parse(assemblyPatternsToInstrument);
            Condition typeInstrumentCondition = new ConditionPatternParser().Parse(typePatternsToInstrument);
            var imd = new Project(originalPathOfDlls, assemblyInstrumentCondition, originalPathOfDlls, typeInstrumentCondition);
            imd.Initialize();
            imd.Accept(new SilverlightProfilerVisitor(originalPathOfDlls, mainSilverlightAssembly, silverlightStartupType, methodToAddProfilingHook));
        }
    }
}