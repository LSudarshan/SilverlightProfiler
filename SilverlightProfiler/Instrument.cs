using System;
using System.IO;
using System.Linq;
using System.Xml;
using Ionic.Zip;
using NMetrics;
using NMetrics.Filters;
using SilverlightProfilerRuntime;
using SilverlightProfilerUnitTest;

namespace SilverlightProfiler
{
    public class Instrument
    {
        private static void Main(string[] args)
        {
            if (args.Length != 4)
            {
                Console.WriteLine(
                    "Usage is : Instrument mainSilverlightAssembly originalPathOfDlls assemblyPatternsToInstrument ClassPatternsToInstrument");
                return;
            }

            string mainSilverlightAssembly = args[0];
            string xapLocation = args[1];
            string assemblyPatternsToInstrument = args[2];
            string typePatternsToInstrument = args[3];

            ExtractXapToTempLocation(xapLocation, "temp");
            string beforeModification = "beforeModification";
            RecreateDirectory(beforeModification);
            string aftermodification = "afterModification";
            RecreateDirectory(aftermodification);
            InstrumentAssemblies(assemblyPatternsToInstrument, typePatternsToInstrument, mainSilverlightAssembly, "temp",beforeModification, aftermodification);
            CopyProfilerRuntimeAssembly("SilverlightProfilerRuntime.dll", "temp");
            CopyInstrumentedAssemblies(aftermodification, "temp");
            FixManifestFile("temp\\AppManifest.xaml");
            ZipTheXapFile("temp", xapLocation);
        }

        private static void RecreateDirectory(string dir)
        {
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
            Directory.CreateDirectory(dir);
        }

        private static void ZipTheXapFile(string sourceFolder, string xapFile)
        {
            using (var zipFile = new ZipFile())
            {
                zipFile.AddFiles(Directory.GetFiles(sourceFolder), false, "");
                zipFile.Save(xapFile);
            }
        }

        private static void FixManifestFile(string manifestFile)
        {
            var document = new XmlDocument();
            document.Load(manifestFile);
            XmlNamespaceManager manager = new XmlNamespaceManager(document.NameTable);
            manager.AddNamespace("x", "http://schemas.microsoft.com/winfx/2006/xaml");
            manager.AddNamespace("", "http://schemas.microsoft.com/client/2007/deployment");
            var deploymentParts = (XmlElement) document.SelectSingleNode("//*[local-name()='Deployment.Parts']");
            XmlElement assemblyPart = document.CreateElement("AssemblyPart", "http://schemas.microsoft.com/client/2007/deployment");
            XmlAttribute nameAttribute = document.CreateAttribute("Name", "http://schemas.microsoft.com/winfx/2006/xaml");
            nameAttribute.Value = "SilverlightProfilerRuntime";
            assemblyPart.Attributes.Append(nameAttribute);
            XmlAttribute sourceAttribute = document.CreateAttribute("Source", "http://schemas.microsoft.com/client/2007/deployment");
            sourceAttribute.Value = "SilverlightProfilerRuntime.dll";
            assemblyPart.Attributes.Append(sourceAttribute);
            deploymentParts.AppendChild(assemblyPart);
            document.Save(manifestFile);
        }

        private static void CopyInstrumentedAssemblies(string sourceFolder, string destFolder)
        {
            Directory.GetFiles(sourceFolder).ToList().ForEach(
                file => File.Copy(file, Path.Combine(destFolder, Path.GetFileName(file)), true));
        }

        private static void CopyProfilerRuntimeAssembly(string profilerRuntimeAssembly, string pathToExtractedAssemblies)
        {
            File.Copy(profilerRuntimeAssembly, Path.Combine(pathToExtractedAssemblies, profilerRuntimeAssembly), true);
        }

        private static void ExtractXapToTempLocation(string xapLocation, string pathtoExtractAssembliesTo)
        {
            if (Directory.Exists(pathtoExtractAssembliesTo)) Directory.Delete(pathtoExtractAssembliesTo, true);
            Directory.CreateDirectory(pathtoExtractAssembliesTo);
            using (var file = new ZipFile(xapLocation))
            {
                file.ExtractAll(pathtoExtractAssembliesTo, ExtractExistingFileAction.OverwriteSilently);
            }
        }

        private static void InstrumentAssemblies(string assemblyPatternsToInstrument, string typePatternsToInstrument,
                                                 string mainSilverlightAssembly, string locationOfDlls,
                                                 string cleanCopyOfDllsLocation, string instrumentedDllsLocation)
        {
            Condition assemblyInstrumentCondition = new ConditionPatternParser().Parse(assemblyPatternsToInstrument);
            Condition typeInstrumentCondition = new ConditionPatternParser().Parse(typePatternsToInstrument);
            var imd = new Project(locationOfDlls, assemblyInstrumentCondition, cleanCopyOfDllsLocation,
                                  typeInstrumentCondition);
            imd.Initialize();
            imd.Accept(new SilverlightProfilerVisitor(instrumentedDllsLocation, mainSilverlightAssembly));
        }
    }
}