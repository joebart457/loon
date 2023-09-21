using Loon.Compiler.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loon.Compiler.Extensions
{
    internal static class ForeignFunctionReferenceListExtensions
    {
        public static string GenerateAssembly(this List<ForeignFunctionReference> ffis, int indentLevel = 0)
        {
            var sb = new StringBuilder();
            var grouped = ffis.GroupBy(f => (f.LibraryAlias, f.LibraryPath));
            sb.AppendLine("library \\".Indent(indentLevel));
         
            sb.AppendLine(string.Join(", \\\r\n", grouped.Select(g => $"{g.Key.LibraryAlias}, '{g.Key.LibraryPath}'".Indent(indentLevel + 1))));
            foreach(var group in grouped)
            {
                sb.AppendLine($"import {group.Key.LibraryAlias}, \\".Indent(indentLevel));
                sb.AppendLine(string.Join(", \\\r\n", group.Select(ffi => $"{ffi.FunctionName}, '{ffi.FunctionAlias}'".Indent(indentLevel+1))));
            }
            return sb.ToString();
        }
    }
}
