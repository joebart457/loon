using Crater.Shared.Models;
using Loon.Analyzer.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loon.Analyzer.Models
{
    public class BlockStatement: ResolvedStatement
    {
        public List<ResolvedStatement> ChildStatements { get; set; }

        public BlockStatement(List<ResolvedStatement> childStatements)
        {
            ChildStatements = childStatements;
        }

        public override string RegenerateCode(int indentLevel = 0)
        {
            var sb = new StringBuilder();
            sb.AppendLine("{".Indent(indentLevel));
            foreach (var statement in ChildStatements)
            {
                sb.AppendLine(statement.RegenerateCode(indentLevel+1));
            }
            sb.AppendLine("}".Indent(indentLevel));
            return sb.ToString();
        }
    }
}
