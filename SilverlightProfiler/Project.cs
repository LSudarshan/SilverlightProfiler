using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using SilverlightProfiler.Filters;
using SilverlightProfiler.Visitors;

namespace SilverlightProfiler
{
    public class Project
    {
        private readonly Condition assemblyInstrumentationCondition;
        private readonly string cleanCopyOfDllsLocation;
        private readonly Condition typeInstrumentCondition;
        private readonly string originalPathOfDlls;

        public Project(string originalPathOfDlls, Condition fileSelectionPattern, string cleanCopyOfDllsLocation, Condition typeInstrumentCondition)
        {
            this.originalPathOfDlls = originalPathOfDlls;
            this.assemblyInstrumentationCondition = fileSelectionPattern;
            this.cleanCopyOfDllsLocation = cleanCopyOfDllsLocation;
            this.typeInstrumentCondition = typeInstrumentCondition;
        }

        public void Initialize()
        {
            if(originalPathOfDlls != cleanCopyOfDllsLocation)
            {
                if (Directory.Exists(cleanCopyOfDllsLocation)) Directory.Delete(cleanCopyOfDllsLocation, true);
                Directory.CreateDirectory(cleanCopyOfDllsLocation);
                FileInfo[] allDlls = new DirectoryInfo(originalPathOfDlls).GetFiles("*.dll", SearchOption.AllDirectories);
                allDlls.ToList().ForEach(CopyToCleanCopyLocation);
            }
            
        }

        private bool MatchesPattern(FileInfo fileInfo)
        {
            return assemblyInstrumentationCondition.Matches(FileName(fileInfo.FullName));
        }

        private string FileName(string sourcePath)
        {
            return sourcePath.Substring(sourcePath.LastIndexOf("\\") + 1);
        }

        private void CopyToCleanCopyLocation(FileInfo fileInfo)
        {
            string fileName = FileName(fileInfo.FullName);
            string destination = cleanCopyOfDllsLocation + "\\" + fileName;
            File.Copy(fileInfo.FullName, destination, true);
        }

        public void Accept(CodeVisitor visitor)
        {
            FileInfo[] allDlls = new DirectoryInfo(cleanCopyOfDllsLocation).GetFiles("*.dll", SearchOption.AllDirectories);
            List<FileInfo> files = allDlls.ToList().FindAll(MatchesPattern).ToList();
            var assemblies =
                new List<AssemblyDefinition>(files.Select(file => AssemblyFactory.GetAssembly(Path.Combine(cleanCopyOfDllsLocation, file.Name))));
            foreach (AssemblyDefinition assembly in assemblies)
            {
                ProcessAssembly(visitor, assembly);
            }
        }

        public void ProcessAssembly(CodeVisitor visitor, AssemblyDefinition assembly)
        {
            visitor.StartVisitingAssemblyDefinition(assembly);
            new StrongName(assembly, assemblyInstrumentationCondition).FixReferences();
            TypeDefinitionCollection types = assembly.MainModule.Types;
            foreach (TypeDefinition type in types)
            {
                if(typeInstrumentCondition.Matches(type.Name))
                    ProcessType(type, visitor);
            }
            visitor.FinishVisitingAssemblyDefinition(assembly);
        }

        private static void ProcessType(TypeDefinition type, CodeVisitor visitor)
        {
            visitor.StartVisitingTypeDefinition(type);
            foreach (FieldDefinition field in type.Fields)
            {
                visitor.VisitFieldDefinition(field);
            }
            foreach (MethodDefinition method in type.Methods)
            {
                visitor.VisitMethodDefinition(method);
            }
            foreach (MethodDefinition constructor in type.Constructors)
            {
                visitor.VisitMethodDefinition(constructor);
            }
            visitor.FinishVisitingTypeDefinition(type);
        }
    }
}