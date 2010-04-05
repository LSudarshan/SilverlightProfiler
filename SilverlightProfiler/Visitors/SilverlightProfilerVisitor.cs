using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using Mono.Cecil;
using Mono.Cecil.Cil;
using SilverlightProfilerRuntime;

namespace SilverlightProfiler.Visitors
{
    public class SilverlightProfilerVisitor : CodeVisitor
    {
        private readonly string profiledDllPath;
        private string mainSilverlightAssembly;

        public SilverlightProfilerVisitor(string profiledDllPath, string mainSilverlightAssembly)
        {
            this.profiledDllPath = profiledDllPath;
            this.mainSilverlightAssembly = mainSilverlightAssembly;
            if (!Directory.Exists(profiledDllPath)) Directory.CreateDirectory(profiledDllPath);
        }

        public override void VisitMethodDefinition(MethodDefinition method)
        {
            if (method.Body == null) return;
            var instructions = new List<Instruction>(method.Body.Instructions.Cast<Instruction>());

            if (instructions.FindAll(instruction => instruction.OpCode == OpCodes.Ret).Count > 1)
            {
                return;
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
            exitInstructions.ForEach(
                delegate(Instruction exitInstruction)
                    {
                        if(exitInstruction.OpCode == OpCodes.Ret)
                        {
                            ReplaceReturnInstructionToAMethodCallToAccomodateBranchesToReturn(exitInstruction, exitingMethod);
                            worker.InsertAfter(exitInstruction, worker.Create(OpCodes.Ret));
                        }
                        else
                        {
                            Instruction exitMethodProfilerInstruction = worker.Create(OpCodes.Call, exitingMethod);
                            worker.InsertBefore(exitInstruction, exitMethodProfilerInstruction);
                        }
                    });
        }

        private void ReplaceReturnInstructionToAMethodCallToAccomodateBranchesToReturn(Instruction exitInstruction, MethodReference exitingMethod)
        {
            exitInstruction.OpCode = OpCodes.Call;
            exitInstruction.Operand = exitingMethod;
        }

        private bool IsExitInstruction(Instruction instruction)
        {
            return instruction.OpCode == OpCodes.Ret || instruction.OpCode == OpCodes.Throw;
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
                TypeDefinition app = assembly.MainModule.Types.Cast<TypeDefinition>().First(definition => definition.BaseType!=null && definition.BaseType.Name.Equals("Application"));

                IEnumerable<MethodDefinition> appMethods = app.Methods.Cast<MethodDefinition>();
                MethodDefinition applicationMethodToInstrument = GetStartupApplicationMethod(appMethods, app.Constructors);/*
                    appMethods.First(definition => definition.Name.Contains(methodToAddProfilingHook));*/

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
            RemoveStrongNames(assembly);
        }

        private MethodDefinition GetStartupApplicationMethod(IEnumerable<MethodDefinition> methods, ConstructorCollection constructors)
        {
            MethodDefinition constructorWithStartupDefinition = constructors.Cast<MethodDefinition>().First(constructor => HasStartupEvent(constructor));
            List<Instruction> instructions = constructorWithStartupDefinition.Body.Instructions.Cast<Instruction>().ToList();
            string operandForLoadingApplicationStartupFunction = instructions[instructions.IndexOf(GetStartupEventHandlerInstruction(instructions))-1].Operand.ToString();
//            Console.WriteLine(operandForLoadingApplicationStartupFunction);
            string applicationStartupMethod = Regex.Match(operandForLoadingApplicationStartupFunction, "::(.*)\\(").Groups[1].Captures[0].Value;
            Console.WriteLine("application startup method is : " + applicationStartupMethod);
            return methods.First(definition => definition.Name == applicationStartupMethod);
        }

        private Instruction GetStartupEventHandlerInstruction(List<Instruction> instructions)
        {
            return instructions.FirstOrDefault(instruction => instruction.OpCode==OpCodes.Newobj && instruction.Operand.ToString().Contains("StartupEventHandler"));
        }

        private bool HasStartupEvent(MethodDefinition constructor)
        {
            return GetStartupEventHandlerInstruction(constructor.Body.Instructions.Cast<Instruction>().ToList()) != null;
        }

        private void RemoveStrongNames(AssemblyDefinition assembly)
        {
            if (assembly.Name.HasPublicKey)
            {
                assembly.Name.PublicKey = new byte[0];
                assembly.Name.PublicKeyToken = new byte[0];
                assembly.Name.Flags = AssemblyFlags.SideBySideCompatible;
                assembly.Name.HasPublicKey = false;
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
            AssemblyFactory.SaveAssembly(assembly, Path.Combine(profiledDllPath, name));
        }
    }
}