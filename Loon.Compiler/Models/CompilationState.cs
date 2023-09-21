using Crater.Shared.Models;
using Loon.Compiler._Generator;
using Loon.Compiler.Extensions;
using Loon.Compiler.Models._CompilationUnit;
using Loon.Compiler.Models._Function;
using Loon.Compiler.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace Loon.Compiler.Models
{
    public class CompilationState
    {
        private List<ForeignFunctionReference> _foreignFunctions = new();
        private List<StaticVariable> _staticData = new();
        private List<CompiledType> _compiledTypes = new();
        private List<CompiledFunction> _compiledFunctions = new();
        private List<AssemblyInclude> _includes = new();
        private EntryPoint? _entry = null;

        // Interm
        private CompiledFunction? _currentProcessDefinition = null;
        public CompilationSettings CompilationSettings { get; private set; } = new();
        public string? LoopStartLabel { get; set; } = null;
        public string? LoopEndLabel { get; set; } = null;
        public void Initialize(CompilationSettings settings)
        {
            CompilationSettings = settings;
            CompilationSettings.AssemblyOutputPath = string.IsNullOrWhiteSpace(CompilationSettings.AssemblyOutputPath) ? 
                Path.GetTempFileName() : 
                SantizeAssemblyFilePath(CompilationSettings.AssemblyOutputPath);
            SantizeOutputFilePath(settings);
            _foreignFunctions = new();
            _staticData = new();
            _compiledTypes = new();
            _compiledFunctions = new();
            _includes = new();
            _entry = null;
            _currentProcessDefinition = null;
        }       

        public void GenerateAssembly()
        {
            if (_entry == null) throw new Exception("entry point is not defined");
            var sb = new StringBuilder();
            if (CompilationSettings.OutputType == OutputType.Exe)
            {
                sb.AppendLine("format PE console");
                sb.AppendLine("entry _start");
            }
            else if (CompilationSettings.OutputType == OutputType.Dll)
            {
                sb.AppendLine("format PE DLL");
                sb.AppendLine("entry DllEntryPoint");
            }
            else throw new Exception($"unable to generate code for output type {CompilationSettings.OutputType}");

            foreach (var include in _includes)
            {
                sb.AppendLine(include.GenerateAssembly(CompilationSettings));
            }

            sb.AppendLine("section '.data' data readable writeable");
            foreach (var type in _compiledTypes)
            {
                sb.AppendLine(type.GenerateAssembly(CompilationSettings, 1));
            }

            foreach (var data in _staticData)
            {
                sb.AppendLine(data.GenerateAssembly(CompilationSettings, 1));
            }

            sb.AppendLine("section '.text' code readable executable");
            sb.AppendLine(_entry.GenerateAssembly(CompilationSettings, 1));
            foreach (var proc in _compiledFunctions)
            {
                sb.AppendLine(proc.GenerateAssembly(CompilationSettings, 1));
            }

            sb.AppendLine("section '.idata' import data readable");
            sb.AppendLine(_foreignFunctions.GenerateAssembly(1));
            if (CompilationSettings.OutputType == OutputType.Dll)
            {
                sb.AppendLine("section '.edata' export data readable");
                _compiledFunctions.GenerateExportList(sb, CompilationSettings, 0);

                sb.AppendLine("section '.reloc' fixups data readable discardable");
                sb.AppendLine("if $= $$".Indent(1));
                sb.AppendLine("dd 0,8; if there are no fixups, generate dummy entry".Indent(2));
                sb.AppendLine("end if".Indent(1));
            }
            

            File.WriteAllText(CompilationSettings.AssemblyOutputPath, sb.ToString());
        }

        public string? GenerateExecutable()
        {
            return FasmService.RunFasm(CompilationSettings);
        }

        private static string SantizeAssemblyFilePath(string outputPath)
        {
            outputPath = Path.GetFullPath(outputPath);
            if (Path.GetExtension(outputPath) != ".asm") return $"{outputPath}.asm";
            return outputPath;
        }

        private static string SantizeOutputFilePath(CompilationSettings settings)
        {
            if (string.IsNullOrEmpty(settings.FinalOutputPath)) settings.FinalOutputPath = Path.GetFullPath(Path.GetFileNameWithoutExtension(settings.InputFilePath));
            var outputPath = Path.GetFullPath(settings.FinalOutputPath);
            if (settings.OutputType == OutputType.Exe && Path.GetExtension(outputPath) != ".exe") outputPath = $"{outputPath}.exe";
            if (settings.OutputType == OutputType.Dll && Path.GetExtension(outputPath) != ".dll") outputPath = $"{outputPath}.dll";
            settings.FinalOutputPath = outputPath;
            return outputPath;
        }

        internal void Add(AssemblyInclude include)
        {
            _includes.Add(include);
        }

        internal void Add(CompiledFunction compiledFunction)
        {
            _currentProcessDefinition = compiledFunction;
            _compiledFunctions.Add(compiledFunction);
        }

        internal void Add(LocalVariable localVariable)
        {
            if (_currentProcessDefinition == null) throw new InvalidOperationException("cannot add local variable to global scope");
            _currentProcessDefinition.AddLocalVariable(localVariable);
        }

        internal void Add(StaticVariable variable)
        {
            _staticData.Add(variable);
        }

        internal void AddStaticData(CrateType type, object value, out LocalVariable alias)
        {
            var found = _staticData.FirstOrDefault(v => v.CrateType == type && v.Value == value);
            if (found != null) 
            { 
                alias = new LocalVariable(type, found.Symbol);
                return;
            }
            Generator.LocalVariable(type, out alias);
            var staticData = new StaticVariable(type, alias.Symbol, value);
            _staticData.Add(staticData);
        }


        internal void Add(CompilationUnit compilationUnit)
        {
            if (_currentProcessDefinition == null) throw new Exception("current process is null");
            _currentProcessDefinition.AddCompilationUnit(compilationUnit);
        }

        internal void AddForeignFunctionReference(CrateFunction function)
        {
            _foreignFunctions.Add(new ForeignFunctionReference(function));
        }

        internal void AddEntryPoint(CompiledFunction function)
        {
            if (_entry != null) throw new Exception("multiple entry points defined");
            _entry = new EntryPoint(function);
        }

        internal void Add(CrateType type)
        {
             _compiledTypes.Add(new CompiledType(type));
        }


    }
}
