using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NUnit.Framework;
using SilverlightProfiler;
using SilverlightProfiler.Filters;
using SilverlightProfiler.Visitors;
using SilverlightTestApplication;

namespace SilverlightProfilerUnitTest
{
    [TestFixture]
    public class SilverlightProfilerTest
    {
        private static AssemblyDefinition assembly;

        [TestFixtureSetUp]
        public void Setup()
        {
            assembly =
                AssemblyFactory.GetAssembly(
                    @"..\..\..\SilverlightTestApplication\bin\debug\SilverlightTestApplication.dll");
            new Project("", null, "", Condition.Eq(".")).ProcessAssembly(
                new SilverlightProfilerVisitor("afterModification\\", "SilverlightTestApplication"), assembly);
        }

        [Test]
        public void ShouldInstrumentInnerclassesCorrectly()
        {
            TypeDefinition type = assembly.Type(typeof (OuterClass.InnerClass));
            MethodDefinition instrumentedMethod = type.Methods[0];
            var instructions = new List<Instruction>(instrumentedMethod.Body.Instructions.Cast<Instruction>());
            List<Instruction> instructionsCallingOtherMethods =
                instructions.FindAll(instruction => instruction.OpCode == OpCodes.Call);
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