using Loon.Parser.Models.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loon.Parser.Models.Statements
{
    public class VariableDeclarationStatement: StatementBase
    {
        public TypeSymbol? TypeSymbol { get; set; }
        public string VariableName { get; set; }
        public ExpressionBase InitializerValue { get; set; }
        public VariableDeclarationStatement(TypeSymbol? typeSymbol, string variableName, ExpressionBase initializerValue)
        {
            TypeSymbol = typeSymbol;
            VariableName = variableName;
            InitializerValue = initializerValue;
        }

        public override StatementBase ReplaceGenericTypeParameters(Dictionary<TypeSymbol, TypeSymbol> typeParameters)
        {
            return new VariableDeclarationStatement(TypeSymbol?.ReplaceMatchingTypeSymbols(typeParameters), VariableName, InitializerValue.ReplaceGenericTypeParameters(typeParameters));
        }
    }
}
