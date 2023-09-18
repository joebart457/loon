using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TokenizerCore.Interfaces;
using TokenizerCore.Model;

namespace Loon.Parser.Models.Expressions
{
    public class IdentifierExpression: ExpressionBase
    {
        public IToken IdentifierSymbol { get; set; }

        public IdentifierExpression(IToken identifierSymbol)
        {
            IdentifierSymbol = identifierSymbol;
        }
    }
}
