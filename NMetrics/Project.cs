using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using NMetrics.Filters;
using NMetrics.Visitors;

namespace NMetrics
{
    public class Project
    {
        private readonly Condition fileSelectionPattern;
        private string dllsLocation;
        private string path;

        public Project(string path, string projectName, Condition fileSelectionPattern)
        {
            this.path = path;
            this.fileSelectionPattern = fileSelectionPattern;
            dllsLocation = "Temp\\" + projectName;
        }

        public void Initialize()
        {
            if (Directory.Exists(dllsLocation)) Directory.Delete(dllsLocation, true);
            Directory.CreateDirectory(dllsLocation);
            FileInfo[] allDlls = new DirectoryInfo(path).GetFiles("*.dll", SearchOption.AllDirectories);
            allDlls.ToList().ForEach(CopyToTempLocation);
        }

        private void CopyToTempLocation(FileInfo fileInfo)
        {
            string sourcePath = fileInfo.FullName;
            string fileName = sourcePath.Substring(sourcePath.LastIndexOf("\\") + 1);
            string destination = dllsLocation + "\\" + fileName;
            if (!File.Exists(destination) && fileSelectionPattern.Matches(fileName))
            {
                File.Copy(sourcePath, destination, false);
            }
        }

        public void Accept(CodeVisitor visitor)
        {
            var assemblies =
                new List<AssemblyDefinition>(Directory.GetFiles(dllsLocation).Select(s => AssemblyFactory.GetAssembly(s)));
            foreach (AssemblyDefinition assembly in assemblies)
            {
                visitor.StartVisitingAssemblyDefinition(assembly);
                TypeDefinitionCollection types = assembly.MainModule.Types;
                foreach (TypeDefinition type in types)
                {
                    ProcessType(type, visitor);
                }
                visitor.FinishVisitingAssemblyDefinition(assembly);
            }
        }

        private void ProcessType(TypeDefinition type, CodeVisitor visitor)
        {
            visitor.StartVisitingTypeDefinition(type);
            foreach (TypeDefinition nestedType in type.NestedTypes)
            {
                ProcessType(nestedType, visitor);
            }
            foreach (FieldDefinition field in type.Fields)
            {
                visitor.VisitFieldDefinition(field);
            }
            foreach (MethodDefinition method in type.Methods)
            {
                visitor.VisitMethodDefinition(method);
            }
            visitor.FinishVisitingTypeDefinition(type);
        }
    }
}