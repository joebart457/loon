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
        public string VariableName { get; set; }
        public ExpressionBase InitializerValue { get; set; }
        public VariableDeclarationStatement(string variableName, ExpressionBase initializerValue)
        {
            VariableName = variableName;
            InitializerValue = initializerValue;
        }
    }
}
