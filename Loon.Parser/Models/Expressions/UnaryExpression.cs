using Loon.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loon.Parser.Models.Expressions
{
    public class UnaryExpression: ExpressionBase
    {
        public UnaryOperator Operator { get;set; }

        public ExpressionBase Rhs { get; set; }
        public UnaryExpression(UnaryOperator @operator, ExpressionBase rhs)
        {
            Operator = @operator;
            Rhs = rhs;
        }

        public override ExpressionBase ReplaceGenericTypeParameters(Dictionary<TypeSymbol, TypeSymbol> typeParameters)
        {
            return new UnaryExpression(Operator, Rhs.ReplaceGenericTypeParameters(typeParameters));
        }
    }
}
