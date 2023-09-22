using Crater.Shared.Models;
using Loon.Analyzer._Analyzer;
using Loon.Compiler.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loon.Compiler.Models
{
    internal class CompiledType
    {
        public CrateType CrateType { get; private set; }

        public CompiledType(CrateType crateType)
        {
            CrateType = crateType;
        }

        public string GenerateAssembly(CompilationSettings settings, int indentLevel = 0)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"; type {CrateType}");
            sb.AppendLine($"struc {CrateType.GetDecoratedAssemblyName()} {{".Indent(indentLevel));
            foreach (var field in CrateType.Fields)
            {
                sb.AppendLine($".{field.Name} {GetDataDefinition(CrateType)}".Indent(indentLevel + 1));
            }
            sb.AppendLine($"}}".Indent(indentLevel));
            return sb.ToString();
        }


        private static string GetDataDefinition(CrateType type)
        {
            if (type == BuiltinTypes.Int32) return "dd 0";
            if (type == BuiltinTypes.Double) return "dq 0";

            if (type == BuiltinTypes.String) return "dd 0";
            if (type.IsReferenceType) return "dd 0";
            if (!type.IsBuiltin && !type.IsReferenceType) return type.Name;
            throw new Exception($"unable to determine storage class for type {type}");
        }
    }
}
