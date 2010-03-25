using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NMetrics.Visitors;

namespace SilverlightProfilerRuntime
{
    public class SilverlightProfilerVisitor : CodeVisitor
    {
        private readonly string profiledDllPath;
        private string mainSilverlightAssembly;
        private string methodToAddProfilingHook;
        private string silverlightStartupType;

        public SilverlightProfilerVisitor(string profiledDllPath, string mainSilverlightAssembly,
                                          string silverlightStartupType, string methodToAddProfilingHook)
        {
            this.profiledDllPath = profiledDllPath;
            this.mainSilverlightAssembly = mainSilverlightAssembly;
            this.methodToAddProfilingHook = methodToAddProfilingHook;
            this.silverlightStartupType = silverlightStartupType;
            if (!Directory.Exists(profiledDllPath)) Directory.CreateDirectory(profiledDllPath);
        }

        public override void VisitMethodDefinition(MethodDefinition method)
        {
            if (method.Body == null) return;
            var instructions = new List<Instruction>(method.Body.Instructions.Cast<Instruction>());

            if (instructions.FindAll(instruction => instruction.OpCode == OpCodes.Ret).Count > 1)
            {
                throw new ApplicationException("Multiple return points in " + method);
            }

            CilWorker worker = method.Body.CilWorker;

            EnteringMethodInstruction(method, worker);

            ExitingMethodInstruction(method, worker);
        }

        private void ExitingMethodInstruction(MethodDefinition method, CilWorker worker)
        {
            MethodReference exitingMethod =
                method.DeclaringType.Module.Import(typeof (Profiler).GetMethod("ExitingMethod"));
            List<Instruction> exitInstructions = ExitInstructions(method);

            Instruction exitMethodInstruction = worker.Create(OpCodes.Call, exitingMethod);
            if(exitInstructions.Exists(instruction => instruction.OpCode == OpCodes.Ret))
            {
                Instruction newReturn = worker.Create(OpCodes.Ret);
                worker.Append(newReturn);
                worker.InsertBefore(newReturn, exitMethodInstruction);        
            }
            
            exitInstructions.ForEach(
                delegate(Instruction exitInstruction)
                    {
                        if(exitInstruction.OpCode == OpCodes.Ret)
                        {
                            worker.Replace(exitInstruction, worker.Create(OpCodes.Br_S, exitMethodInstruction));
                        } else
                        {
                            worker.InsertBefore(exitInstruction, exitMethodInstruction);    
                        }
                    });
        }

        private bool IsExitInstruction(Instruction instruction)
        {
            return instruction.OpCode == OpCodes.Ret || instruction.OpCode == OpCodes.Throw || instruction.OpCode == OpCodes.Rethrow;
        }

        private void EnteringMethodInstruction(MethodDefinition method, CilWorker worker)
        {
            MethodReference enteringMethod =
                method.DeclaringType.Module.Import(typeof (Profiler).GetMethod("EnteringMethod"));
            worker.InsertBefore(method.Body.Instructions[0], worker.Create(OpCodes.Call, enteringMethod));
        }

        public override void StartVisitingAssemblyDefinition(AssemblyDefinition assembly)
        {
            if (assembly.Name.Name.Contains(mainSilverlightAssembly))
            {
                MethodReference initProfilerMethod = assembly.MainModule.Import(typeof (Profiler).GetMethod("Init"));
                MethodReference getRootVisualMethod =
                    assembly.MainModule.Import(typeof (Application).GetMethod("get_RootVisual"));
                TypeDefinition app = assembly.MainModule.Types[silverlightStartupType];

                IEnumerable<MethodDefinition> appMethods = app.Methods.Cast<MethodDefinition>();
                MethodDefinition applicationMethodToInstrument =
                    appMethods.First(definition => definition.Name.Contains(methodToAddProfilingHook));

                CilWorker worker = applicationMethodToInstrument.Body.CilWorker;

                Instruction loadArgumentToStack = worker.Create(OpCodes.Ldarg_0);
                Instruction getRootVisualInstruction = worker.Create(OpCodes.Call, getRootVisualMethod);
                Instruction callProfilerInstruction = worker.Create(OpCodes.Call, initProfilerMethod);
                Instruction nopInstruction = worker.Create(OpCodes.Nop);

                List<Instruction> exitInstructions = ExitInstructions(applicationMethodToInstrument);
                exitInstructions.ForEach(delegate(Instruction exitInstruction)
                                             {
                                                 worker.InsertBefore(exitInstruction, nopInstruction);
                                                 worker.InsertBefore(nopInstruction, callProfilerInstruction);
                                                 worker.InsertBefore(callProfilerInstruction, getRootVisualInstruction);
                                                 worker.InsertBefore(getRootVisualInstruction, loadArgumentToStack);
                                             });
            }
        }

        private List<Instruction> ExitInstructions(MethodDefinition method)
        {
            return
                method.Body.Instructions.Cast<Instruction>().ToList().FindAll(
                    instruction => IsExitInstruction(instruction)).ToList();
        }

        public override void FinishVisitingAssemblyDefinition(AssemblyDefinition assembly)
        {
            string name = assembly.Name.Name + ".dll";
            AssemblyFactory.SaveAssembly(assembly, profiledDllPath + name);
        }
    }
}