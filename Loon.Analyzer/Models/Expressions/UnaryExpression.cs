using Crater.Shared.Models;
using Loon.Analyzer.Extensions;
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

        public override string RegenerateCode(int indentLevel = 0)
        {
            return $"{RegenerateOperator(Operator)} {Rhs.RegenerateCode(0)}".Indent(indentLevel);
        }

        private string RegenerateOperator(UnaryOperator @operator)
        {
            if (@operator == UnaryOperator.Not) return "!";
            if (@operator == UnaryOperator.Negate) return "-";
            if (@operator == UnaryOperator.StringSize) return "$";
            return "<unknown> ";
        }
    }
}
