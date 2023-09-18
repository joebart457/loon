using Loon.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loon.Parser.Models.Expressions
{
    
    public class BinaryExpression: ExpressionBase
    {
        public BinaryOperator Operator { get; set; }
        public ExpressionBase Lhs { get; set; }
        public ExpressionBase Rhs { get; set; }
        public BinaryExpression(BinaryOperator @operator, ExpressionBase lhs, ExpressionBase rhs)
        {
            Operator = @operator;
            Lhs = lhs;
            Rhs = rhs;
        }
    }
}
