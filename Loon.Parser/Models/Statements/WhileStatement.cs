using Loon.Parser.Models.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loon.Parser.Models.Statements
{
    public class WhileStatement: StatementBase
    {
        public ExpressionBase Condition { get; private set; }
        public StatementBase Then { get; private set; }
        public WhileStatement(ExpressionBase condition, StatementBase then)
        {
            Condition = condition;
            Then = then;
        }
    }
}
