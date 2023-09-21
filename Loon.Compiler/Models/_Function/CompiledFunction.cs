using Crater.Shared.Models;
using Loon.Analyzer._Analyzer;
using Loon.Compiler._Generator;
using Loon.Compiler.Extensions;
using Loon.Compiler.Models._CompilationUnit;
using Loon.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loon.Compiler.Models._Function
{
    internal class CompiledFunction
    {
        public CrateFunction Function { get; private set; }

        public CompiledFunction(CrateFunction function)
        {
            Function = function;
        }

        private List<LocalVariable> _locals = new();
        private List<CompilationUnit> _compilationUnits = new();


        public void AddLocalVariable(LocalVariable variable)
        {
            _locals.Add(variable);
        }

        public void AddCompilationUnit(CompilationUnit compilationUnit)
        {
            _compilationUnits.Add(compilationUnit);
        }

        public string GenerateAssembly(CompilationSettings settings, int indentLevel = 0)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"; {Function.GetSignature()}");
            sb.AppendLine(GetProcessSignature(indentLevel));
            foreach(var local in _locals)
            {
                sb.AppendLine(local.GenerateAssembly(settings, indentLevel + 1));
            }
            sb.AppendLine();
            foreach(var compilationUnit in _compilationUnits)
            {
                sb.AppendLine(compilationUnit.GenerateAssembly(settings, indentLevel + 1));
            }
            sb.AppendLine(Ins.ENDP().Indent(indentLevel));
            return sb.ToString();
        }



        private string GetProcessSignature(int indentLevel = 0)
        {
            var sb = new StringBuilder();
            var callingConvention = Function.CallingConvention == CallingConvention.StdCall ? "stdcall" : "";
            var parameters = string.Join(", ", Function.Parameters.Select(p => ParameterHelper(p)));
            sb.AppendLine($"proc {Function.Name} {callingConvention} {parameters}".Indent(indentLevel));
            return sb.ToString();
        }

        private static string ParameterHelper(CrateParameterInfo paramInfo)
        {
            return $"{paramInfo.Name.Decorate()}:{GetAssemblyParameterType(paramInfo.CrateType)}";
        }

        private static string GetAssemblyParameterType(CrateType type)
        {
            if (type == BuiltinTypes.Double) return "qword";
            if (type == BuiltinTypes.String) return "dword";
            if (type == BuiltinTypes.Int32) return "dword";
            if (type == BuiltinTypes.Int16) return "word";
            if (type == BuiltinTypes.Int8) return "byte";
            if (type.IsReferenceType) return "dword";
            if (!type.IsReferenceType && !type.IsBuiltin) return "dword";
            throw new Exception($"unable to determine local assembly parameter type alias for type {type}");
        }
    }
}
