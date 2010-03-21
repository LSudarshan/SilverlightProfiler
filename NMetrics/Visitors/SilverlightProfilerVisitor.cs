using System;
using Mono.Cecil;

namespace NMetrics.Visitors
{
    public class SilverlightProfilerVisitor : CodeVisitor
    {
        public override void VisitMethodDefinition(MethodDefinition method)
        {
            
        }

        public override void FinishVisitingAssemblyDefinition(AssemblyDefinition assembly)
        {
            string name = assembly.Name.Name + ".dll";
            AssemblyFactory.SaveAssembly(assembly, name);
        }
    }
}