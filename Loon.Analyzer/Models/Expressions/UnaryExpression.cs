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
    public class UnaryExpression: TypedExpression
    {
        public UnaryOperator Operator { get;set; }

        public TypedExpression Rhs { get; set; }
        public UnaryExpression(CrateType resultingType, UnaryOperator @operator, TypedExpression rhs)
            :base(resultingType)
        {
            Operator = @operator;
            Rhs = rhs;
        }
    }
}
