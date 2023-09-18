using Crater.Shared.Models;
using Loon.Parser.Models.Expressions;
using Loon.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loon.Analyzer.Models
{

    public class BinaryExpression: TypedExpression
    {
        public BinaryOperator Operator { get; set; }
        public TypedExpression Lhs { get; set; }
        public TypedExpression Rhs { get; set; }
        public BinaryExpression(CrateType resultingType, BinaryOperator @operator, TypedExpression lhs, TypedExpression rhs)
            :base(resultingType)
        {
            Operator = @operator;
            Lhs = lhs;
            Rhs = rhs;
        }
    }
}
