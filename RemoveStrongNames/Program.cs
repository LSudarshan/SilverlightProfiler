using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Mono.Cecil;

namespace RemoveStrongNames
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] files = Directory.GetFiles(@"E:\projects\BCG\IMD\tools\Telerik", "*.dll");
            foreach (string file in files)
            {
                RemoveStrongName(file, "Telerik");
            }
        }

        
        private static void RemoveStrongName(string file, string patternToFix)
        {
            AssemblyDefinition assembly = AssemblyFactory.GetAssembly(file);

            AssemblyNameDefinition name = assembly.Name;
            if (name.HasPublicKey)
            {
                RemoveStrongName(name);
            }
            foreach (AssemblyNameReference reference in assembly.MainModule.AssemblyReferences)
            {
                if(reference.Name.Contains(patternToFix)) 
                    RemoveStrongName(reference);
            }
            FixInternalsVisibleToCustomAttributes(assembly);
            AssemblyFactory.SaveAssembly(assembly, file);
        }

        private static void FixInternalsVisibleToCustomAttributes(AssemblyDefinition assembly)
        {
            List<CustomAttribute> customAttributes = new List<CustomAttribute>(assembly.CustomAttributes.Cast<CustomAttribute>());
            List<CustomAttribute> internalsVisibleAttributes = customAttributes.FindAll(attribute => attribute.Constructor.DeclaringType.Name.Contains("InternalsVisibleToAttribute"));
            List<CustomAttribute> telerikAssembliesAttributes = internalsVisibleAttributes.FindAll(attribute => ((string)attribute.ConstructorParameters[0]).Contains("Telerik"));
            telerikAssembliesAttributes.ForEach(attribute => RemovePublicKey(attribute));
        }

        private static void RemovePublicKey(CustomAttribute attribute)
        {
            string assemblyReference = (string) attribute.ConstructorParameters[0];
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
