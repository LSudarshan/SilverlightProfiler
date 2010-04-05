using Mono.Cecil;
using NMetrics.Filters;

namespace NMetrics.Visitors
{
    public class CodeVisitor
    {
        public CodeVisitor()
        {
        }

        public virtual void StartVisitingAssemblyDefinition(AssemblyDefinition assembly)
        {
        }

        public virtual void FinishVisitingAssemblyDefinition(AssemblyDefinition assembly)
        {
        }

        public virtual void StartVisitingTypeDefinition(TypeDefinition type)
        {
        }

        public virtual void FinishVisitingTypeDefinition(TypeDefinition type)
        {
        }

        public virtual void VisitMethodDefinition(MethodDefinition method)
        {
        }

        public virtual void VisitFieldDefinition(FieldDefinition field)
        {
        }
    }
}