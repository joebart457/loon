using Loon.Parser.Models.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loon.Parser.Models.Statements
{
    public class ExpressionStatement: StatementBase
    {
        
        public ExpressionBase Expression { get; set; }

        public ExpressionStatement(ExpressionBase expression)
        {
            Expression = expression;
        }
    }
}
