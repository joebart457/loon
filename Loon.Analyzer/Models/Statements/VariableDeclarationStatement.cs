using Crater.Shared.Models;
using Loon.Parser.Models.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loon.Analyzer.Models
{
    public class VariableDeclarationStatement: ResolvedStatement
    {
        public CrateType VariableType { get; set; }
        public string VariableName { get; set; }
        public TypedExpression InitializerValue { get; set; }
        public VariableDeclarationStatement(CrateType variableType, string variableName, TypedExpression initializerValue)
        {
            VariableType = variableType;
            VariableName = variableName;
            InitializerValue = initializerValue;
        }
    }
}
