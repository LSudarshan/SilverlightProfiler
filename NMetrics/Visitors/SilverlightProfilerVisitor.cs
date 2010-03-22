using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;
using SilverlightProfiler;

namespace NMetrics.Visitors
{
    public class SilverlightProfilerVisitor : CodeVisitor
    {
        private readonly string profiledDllPath;

        public SilverlightProfilerVisitor(string profiledDllPath)
        {
            this.profiledDllPath = profiledDllPath;
            if (!Directory.Exists(profiledDllPath)) Directory.CreateDirectory(profiledDllPath);
        }

        public override void VisitMethodDefinition(MethodDefinition method)
        {
            if(method.Body == null) return;
            List<Instruction> instructions = new List<Instruction>(method.Body.Instructions.Cast<Instruction>());

            if(instructions.FindAll(instruction => instruction.OpCode ==OpCodes.Ret).Count > 1)
            {
                throw new ApplicationException("Multiple return points in " + method);
            }
        }

        public override void StartVisitingAssemblyDefinition(AssemblyDefinition assembly)
        {
            if(assembly.Name.Name.Contains("IMDClient"))
            {
                MethodReference initProfilerMethod = assembly.MainModule.Import(typeof(Profiler).GetMethod("Init"));
                MethodReference getRootVisualMethod = assembly.MainModule.Import(typeof(Application).GetMethod("get_RootVisual"));
                TypeDefinition app = assembly.MainModule.Types["IMDClient.App"];

                IEnumerable<MethodDefinition> appMethods = app.Methods.Cast<MethodDefinition>();
                MethodDefinition applicationMethodToInstrument = appMethods.First(definition => definition.Name.Contains("ApplicationStartup"));

                CilWorker worker = applicationMethodToInstrument.Body.CilWorker;

                Instruction loadArgumentToStack = worker.Create(OpCodes.Ldarg_0);
                Instruction getRootVisualInstruction = worker.Create(OpCodes.Call, getRootVisualMethod);
                Instruction callProfilerInstruction = worker.Create(OpCodes.Call, initProfilerMethod);
                Instruction nopInstruction = worker.Create(OpCodes.Nop);

                Instruction returnInstruction = applicationMethodToInstrument.Body.Instructions.Cast<Instruction>().First(instruction1 => instruction1.OpCode == OpCodes.Ret);
                worker.InsertBefore(returnInstruction, nopInstruction);
                worker.InsertBefore(nopInstruction, callProfilerInstruction);
                worker.InsertBefore(callProfilerInstruction, getRootVisualInstruction);
                worker.InsertBefore(getRootVisualInstruction, loadArgumentToStack);
            }
        }

        public override void FinishVisitingAssemblyDefinition(AssemblyDefinition assembly)
        {
            string name = assembly.Name.Name + ".dll";
            AssemblyFactory.SaveAssembly(assembly, profiledDllPath + name);
        }
    }
}