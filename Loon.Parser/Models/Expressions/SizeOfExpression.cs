using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loon.Parser.Models.Expressions
{
    public class SizeOfExpression: ExpressionBase
    {
        public TypeSymbol TypeSymbol { get; private set; }

        public SizeOfExpression(TypeSymbol typeSymbol)
        {
            TypeSymbol = typeSymbol;
        }
    }
}
