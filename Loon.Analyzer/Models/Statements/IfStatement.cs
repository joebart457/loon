using Crater.Shared.Models;
using Loon.Analyzer.Extensions;
using Loon.Parser.Models.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loon.Analyzer.Models
{
    public class IfStatement: ResolvedStatement
    {
        public TypedExpression Condition { get; set; }
        public ResolvedStatement Then { get; set; }
        public ResolvedStatement? Else { get; set; }
        public IfStatement(TypedExpression condition, ResolvedStatement then, ResolvedStatement? @else)
        {
            Condition = condition;
            Then = then;
            Else = @else;
        }
        public override string RegenerateCode(int indentLevel = 0)
        {
            var sb = new StringBuilder();
            
            sb.AppendLine($"if ({Condition.RegenerateCode(0)})");
            sb.AppendLine(Then.RegenerateCode(indentLevel));
            if (Else != null)
            {
                sb.AppendLine("else".Indent(indentLevel));
                sb.AppendLine(Else.RegenerateCode(indentLevel));
            }
            return sb.ToString();
        }


    }
}
