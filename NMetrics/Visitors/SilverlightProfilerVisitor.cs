using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;
using SilverlightProfilerRuntime;


namespace NMetrics.Visitors
{
    public class SilverlightProfilerVisitor : CodeVisitor
    {
        private readonly string profiledDllPath;
        private string mainSilverlightAssembly;
        private string methodToAddProfilingHook;
        private string silverlightStartupType;

        public SilverlightProfilerVisitor(string profiledDllPath, string mainSilverlightAssembly, string methodToAddProfilingHook1, string silverlightStartupType1)
        {
            this.profiledDllPath = profiledDllPath;
            this.mainSilverlightAssembly = mainSilverlightAssembly;
            this.methodToAddProfilingHook = methodToAddProfilingHook1;
            this.silverlightStartupType = silverlightStartupType1;
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
            if(assembly.Name.Name.Contains(mainSilverlightAssembly))
            {
                MethodReference initProfilerMethod = assembly.MainModule.Import(typeof(Profiler).GetMethod("Init"));
                MethodReference getRootVisualMethod = assembly.MainModule.Import(typeof(Application).GetMethod("get_RootVisual"));
                TypeDefinition app = assembly.MainModule.Types[silverlightStartupType];

                IEnumerable<MethodDefinition> appMethods = app.Methods.Cast<MethodDefinition>();
                MethodDefinition applicationMethodToInstrument = appMethods.First(definition => definition.Name.Contains(methodToAddProfilingHook));

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