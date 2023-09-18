using Loon.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loon.Compiler.Extensions
{
    internal static class OperatorExtensions
    {
        public static bool IsComparison(this BinaryOperator op)
        {
            return op == BinaryOperator.LessThan || op == BinaryOperator.GreaterThan || op == BinaryOperator.LessThanEqual
                || op == BinaryOperator.GreaterThanEqual || op == BinaryOperator.Equal || op == BinaryOperator.NotEqual;
        }
    }
}
