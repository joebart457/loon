using Crater.Shared.Models;
using Loon.Analyzer.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loon.Analyzer.Models
{
    public class InlineAssemblyStatement: ResolvedStatement
    {
        public string Asm { get; private set; }

        public InlineAssemblyStatement(string asm)
        {
            Asm = asm;
        }

        public override string RegenerateCode(int indentLevel = 0)
        {
            var sb = new StringBuilder();
            sb.AppendLine("__asm {{".Indent(indentLevel));
            sb.AppendLine(Asm.ReplaceLineEndings($"\r\n{"".Indent(indentLevel + 1)}"));
            sb.AppendLine("}}".Indent(indentLevel));
            return sb.ToString();
        }
    }
}
