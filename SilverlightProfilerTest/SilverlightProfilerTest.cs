using System;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NMetrics;
using NMetrics.Filters;
using NMetrics.Visitors;
using NUnit.Framework;
using SampleSilverlightApplication;
using SilverlightProfilerRuntime;
using System.Linq;

namespace SilverlightProfilerUnitTest
{
    [TestFixture]
    public class SilverlightProfilerTest
    {
        private static AssemblyDefinition assembly;

        [TestFixtureSetUp]
        public void Setup()
        {
            assembly = AssemblyFactory.GetAssembly(@"..\..\..\SampleSilverlightApplication\bin\debug\SampleSilverlightApplication.dll");
            new Project("", null, "", Condition.Eq(".")).ProcessAssembly(new SilverlightProfilerVisitor("afterModification\\", "SammpleSilverlightApplication"), assembly);
        }

        [Test]
        public void ShouldInstrumentInnerclassesCorrectly()
        {
            TypeDefinition type = assembly.Type(typeof(OuterClass.InnerClass));
            MethodDefinition instrumentedMethod = type.Methods[0];
            List<Instruction> instructions = new List<Instruction>(instrumentedMethod.Body.Instructions.Cast<Instruction>());
            List<Instruction> instructionsCallingOtherMethods = instructions.FindAll(instruction => instruction.OpCode == OpCodes.Call);
            instructionsCallingOtherMethods.ShouldBeOfSize(2); 
            instructionsCallingOtherMethods[0].Operand.ToString().ShouldContain("EnteringMethod");
            instructionsCallingOtherMethods[1].Operand.ToString().ShouldContain("ExitingMethod");
        }
    }

    public static class MonoX
    {
        public static TypeDefinition Type(this AssemblyDefinition assembly, Type type)
        {
            return assembly.MainModule.Types.Cast<TypeDefinition>().First(definition => definition.Name == type.Name);
        }
    }
}