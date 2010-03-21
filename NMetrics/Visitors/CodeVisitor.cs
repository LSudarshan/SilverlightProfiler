using Mono.Cecil;

namespace NMetrics.Visitors
{
    public class CodeVisitor
    {
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