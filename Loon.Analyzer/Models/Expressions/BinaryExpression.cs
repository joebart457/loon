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

        public override string RegenerateCode(int indentLevel = 0)
        {
            return $"{Lhs.RegenerateCode(0)} {RegenerateOperator(Operator)} {Rhs.RegenerateCode(0)}".Indent(indentLevel);
        }

        private string RegenerateOperator(BinaryOperator @operator)
        {
            if (@operator == BinaryOperator.And) return "&&";
            if (@operator == BinaryOperator.Or) return "||";
            if (@operator == BinaryOperator.NotEqual) return "!=";
            if (@operator == BinaryOperator.Equal) return "==";
            if (@operator == BinaryOperator.LessThan) return "<";
            if (@operator == BinaryOperator.LessThanEqual) return "<=";
            if (@operator == BinaryOperator.GreaterThan) return ">";
            if (@operator == BinaryOperator.GreaterThanEqual) return ">=";
            if (@operator == BinaryOperator.Add) return "+";
            if (@operator == BinaryOperator.Subtract) return "-";
            if (@operator == BinaryOperator.Multiply) return "*";
            if (@operator == BinaryOperator.Divide) return "/";
            return "<unknown> ";

        }
    }
}
