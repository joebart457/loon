using Crater.Shared.Models;
using Loon.Analyzer._Analyzer;
using Loon.Compiler.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loon.Compiler.Models._Function
{
    internal class LocalVariable
    {
        public bool IsDiscard { get; private set; } = false;
        public CrateType CrateType { get; private set; }
        public string Symbol { get; private set; }
        public LocalVariable(CrateType crateType, string symbol, bool isDiscard = false)
        {
            CrateType = crateType;
            Symbol = symbol;
            IsDiscard = isDiscard;
        }

        public string GenerateAssembly(CompilationSettings settings, int indentLevel = 0)
        {
            return $"local {Symbol}:{GetLocalVariableType(CrateType)} ; local for type {CrateType}".Indent(indentLevel);
        }

        private static string GetLocalVariableType(CrateType type)
        {
            if (type == BuiltinTypes.Int8) return "BYTE";
            if (type == BuiltinTypes.Int16) return "WORD";
            if (type == BuiltinTypes.Double) return "QWORD";
            if (type == BuiltinTypes.String) return "DWORD";
            if (type == BuiltinTypes.Int32) return "DWORD";
            if (type.IsReferenceType) return "DWORD";
            if (!type.IsBuiltin && !type.IsReferenceType) return "DWORD";
            throw new Exception($"unable to determine local assembly type alias for type {type}");
        }
    }
}
