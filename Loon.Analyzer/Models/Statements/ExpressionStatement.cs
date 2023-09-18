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
    }
}
