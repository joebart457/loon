using Crater.Shared.Models;
using Loon.Parser.Models.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loon.Analyzer.Models
{
    public class ExpressionStatement: ResolvedStatement
    {
        
        public TypedExpression Expression { get; set; }

        public ExpressionStatement(TypedExpression expression)
        {
            Expression = expression;
        }

        public override string RegenerateCode(int indentLevel = 0)
        {
            return $"{Expression.RegenerateCode(indentLevel)};";
        }
    }
}
