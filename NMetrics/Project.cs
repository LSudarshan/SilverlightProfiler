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
        private readonly string updatedDllsLocation;
        private readonly string path;

        public Project(string path, Condition fileSelectionPattern, string updatedDllsLocation)
        {
            this.path = path;
            this.fileSelectionPattern = fileSelectionPattern;
            this.updatedDllsLocation = updatedDllsLocation;
        }

        public void Initialize()
        {
            if(path != updatedDllsLocation)
            {
                if (Directory.Exists(updatedDllsLocation)) Directory.Delete(updatedDllsLocation, true);
                Directory.CreateDirectory(updatedDllsLocation);
                FileInfo[] allDlls = new DirectoryInfo(path).GetFiles("*.dll", SearchOption.AllDirectories);
                allDlls.ToList().ForEach(CopyToTempLocation);
            }
            
        }

        private bool MatchesPattern(FileInfo fileInfo)
        {
            return fileSelectionPattern.Matches(FileName(fileInfo.FullName));
        }

        private string FileName(string sourcePath)
        {
            return sourcePath.Substring(sourcePath.LastIndexOf("\\") + 1);
        }

        private void CopyToTempLocation(FileInfo fileInfo)
        {
            string fileName = FileName(fileInfo.FullName);
            string destination = updatedDllsLocation + "\\" + fileName;
            File.Copy(fileInfo.FullName, destination, true);
        }

        public void Accept(CodeVisitor visitor)
        {
            FileInfo[] allDlls = new DirectoryInfo(path).GetFiles("*.dll", SearchOption.AllDirectories);
            List<FileInfo> files = allDlls.ToList().FindAll(MatchesPattern).ToList();
            var assemblies =
                new List<AssemblyDefinition>(files.Select(file => AssemblyFactory.GetAssembly(updatedDllsLocation + file.Name)));
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