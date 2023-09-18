using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TokenizerCore.Model;

namespace Loon.Parser.Models.Expressions
{
    public class LiteralExpression: ExpressionBase
    {
        public object Value { get; set; }

        public LiteralExpression(object value)
        {
            Value = value;
        }
    }
}
