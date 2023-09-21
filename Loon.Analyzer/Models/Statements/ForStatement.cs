using Crater.Shared.Models;
using Loon.Analyzer.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loon.Analyzer.Models
{
    public class ForStatement: ResolvedStatement
    {
        public ResolvedStatement? Initializer { get; private set; }
        public TypedExpression? Condition { get; private set; }
        public TypedExpression? Iterator { get; private set; }
        public ResolvedStatement Then { get; private set; }
        public ForStatement(ResolvedStatement? initializer, TypedExpression? condition, TypedExpression? iterator, ResolvedStatement then)
        {
            Initializer = initializer;
            Condition = condition;
            Iterator = iterator;
            Then = then;
        }

        public override string RegenerateCode(int indentLevel = 0)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"for({Initializer?.RegenerateCode(0)}{Condition?.RegenerateCode(0)};{Iterator?.RegenerateCode(0)})".Indent(indentLevel));
            sb.AppendLine(Then.RegenerateCode(indentLevel + 1));
            return sb.ToString();
        }
    }
}
