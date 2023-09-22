using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loon.Parser.Models.Expressions
{
    public class TypeInitializerExpression : ExpressionBase
    {
        public TypeSymbol TypeSymbol { get; set; }
        public TypeInitializerExpression(TypeSymbol typeSymbol)
        {
            TypeSymbol = typeSymbol;
        }

        public override ExpressionBase ReplaceGenericTypeParameters(Dictionary<TypeSymbol, TypeSymbol> typeParameters)
        {
            return new TypeInitializerExpression(TypeSymbol.ReplaceMatchingTypeSymbols(typeParameters));
        }
    }
}
