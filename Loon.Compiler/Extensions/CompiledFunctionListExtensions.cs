using Loon.Compiler.Models;
using Loon.Compiler.Models._Function;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loon.Compiler.Extensions
{
    internal static class CompiledFunctionListExtensions
    {
        public static void GenerateExportList(this List<CompiledFunction> compiledFunctions, StringBuilder sb, CompilationSettings settings, int indentLevel = 0)
        {
            sb.AppendLine($"export '{Path.GetFileName(settings.FinalOutputPath)}', \\".Indent(indentLevel + 1));
            sb.AppendLine(string.Join(", \\\r\n", compiledFunctions.Where(cf => cf.Function.IsExport).Select(cf => $"{cf.Function.Name}, '{cf.Function.Name}'".Indent(indentLevel + 2))));
        }
    }
}
