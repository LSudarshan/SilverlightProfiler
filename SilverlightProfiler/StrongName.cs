using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Mono.Cecil;
using SilverlightProfiler.Filters;

namespace SilverlightProfiler
{
    public class StrongName
    {
        private readonly AssemblyDefinition assembly;
        private readonly Condition assemblyInstrumentCondition;

        public StrongName(AssemblyDefinition assembly, Condition assemblyInstrumentCondition)
        {
            this.assembly = assembly;
            this.assemblyInstrumentCondition = assemblyInstrumentCondition;
        }

        public void FixReferences()
        {
            AssemblyNameDefinition name = assembly.Name;
            if (name.HasPublicKey)
            {
                RemoveStrongName(name);
            }
            foreach (AssemblyNameReference reference in assembly.MainModule.AssemblyReferences)
            {
                if (assemblyInstrumentCondition.Matches(reference.Name) && reference.HasPublicKey)
                    RemoveStrongName(reference);
            }
            FixInternalsVisibleToCustomAttributes(assembly);
        }

        private void FixInternalsVisibleToCustomAttributes(AssemblyDefinition assembly)
        {
            var customAttributes = new List<CustomAttribute>(assembly.CustomAttributes.Cast<CustomAttribute>());
            List<CustomAttribute> internalsVisibleAttributes =
                customAttributes.FindAll(
                    attribute => attribute.Constructor.DeclaringType.Name.Contains("InternalsVisibleToAttribute"));
            List<CustomAttribute> customAttributesWhichNeedToBeFixed =
                internalsVisibleAttributes.FindAll(
                    attribute => assemblyInstrumentCondition.Matches((string) attribute.ConstructorParameters[0]));
            customAttributesWhichNeedToBeFixed.ForEach(attribute => RemovePublicKey(attribute));
        }

        private static void RemovePublicKey(CustomAttribute attribute)
        {
            var assemblyReference = (string) attribute.ConstructorParameters[0];
            attribute.ConstructorParameters[0] = Regex.Replace(assemblyReference, ",\\s*PublicKey=.*", "");
        }

        private static void RemoveStrongName(AssemblyNameReference name)
        {
            name.PublicKey = null;
            name.PublicKeyToken = new byte[0];
            name.HasPublicKey = false;
            name.Flags &= ~AssemblyFlags.PublicKey;
            name.HashAlgorithm = AssemblyHashAlgorithm.None;
        }
    }
}