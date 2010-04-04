﻿using System.Collections.Generic;
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
        private readonly string cleanCopyOfDllsLocation;
        private readonly string originalPathOfDlls;

        public Project(string originalPathOfDlls, Condition fileSelectionPattern, string cleanCopyOfDllsLocation)
        {
            this.originalPathOfDlls = originalPathOfDlls;
            this.fileSelectionPattern = fileSelectionPattern;
            this.cleanCopyOfDllsLocation = cleanCopyOfDllsLocation;
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
            return fileSelectionPattern.Matches(FileName(fileInfo.FullName));
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
                new List<AssemblyDefinition>(files.Select(file => AssemblyFactory.GetAssembly(cleanCopyOfDllsLocation + file.Name)));
            foreach (AssemblyDefinition assembly in assemblies)
            {
                ProcessAssembly(visitor, assembly);
            }
        }

        public static void ProcessAssembly(CodeVisitor visitor, AssemblyDefinition assembly)
        {
            visitor.StartVisitingAssemblyDefinition(assembly);
            TypeDefinitionCollection types = assembly.MainModule.Types;
            foreach (TypeDefinition type in types)
            {
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