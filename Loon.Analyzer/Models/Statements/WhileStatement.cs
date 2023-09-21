using Crater.Shared.Models;
using Loon.Analyzer.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loon.Analyzer.Models
{
    public class WhileStatement: ResolvedStatement
    {
        public TypedExpression Condition { get; private set; }
        public ResolvedStatement Then { get; private set; }
        public WhileStatement(TypedExpression condition, ResolvedStatement then)
        {
            Condition = condition;
            Then = then;
        }

        public override string RegenerateCode(int indentLevel = 0)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"while({Condition.RegenerateCode(0)})".Indent(indentLevel));
            sb.AppendLine(Then.RegenerateCode(indentLevel + 1));
            return sb.ToString();
        }

    }
}
