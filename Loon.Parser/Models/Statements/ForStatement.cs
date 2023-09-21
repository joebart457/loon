using Loon.Parser.Models.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loon.Parser.Models.Statements
{
    public class ForStatement: StatementBase
    {
        public StatementBase? Initializer { get; private set; }
        public ExpressionBase? Condition { get; private set; }
        public ExpressionBase? Iterator { get; private set; }
        public StatementBase Then { get; private set; }
        public ForStatement(StatementBase? initializer, ExpressionBase? condition, ExpressionBase? iterator, StatementBase then)
        {
            Initializer = initializer;
            Condition = condition;
            Iterator = iterator;
            Then = then;
        }
    }
}
