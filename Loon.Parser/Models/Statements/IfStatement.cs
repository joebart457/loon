using Loon.Parser.Models.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loon.Parser.Models.Statements
{
    
    public class IfStatement: StatementBase
    {
        public ExpressionBase Condition { get; set; }
        public StatementBase Then { get; set; }
        public StatementBase? Else { get; set; }
        public IfStatement(ExpressionBase condition, StatementBase then, StatementBase? @else)
        {
            Condition = condition;
            Then = then;
            Else = @else;
        }

        public override StatementBase ReplaceGenericTypeParameters(Dictionary<TypeSymbol, TypeSymbol> typeParameters)
        {
            return new IfStatement(Condition.ReplaceGenericTypeParameters(typeParameters), Then.ReplaceGenericTypeParameters(typeParameters), Else?.ReplaceGenericTypeParameters(typeParameters));
        }
    }
}
