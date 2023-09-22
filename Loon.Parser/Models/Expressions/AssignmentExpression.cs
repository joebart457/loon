using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loon.Parser.Models.Expressions
{
    public class AssignmentExpression: ExpressionBase
    {
        public ExpressionBase? InstanceTarget { get; set; }
        public string AssignmentTarget { get; set; }
        public ExpressionBase ValueToAssign { get; set; }
        public AssignmentExpression(ExpressionBase? instanceTarget, string assignmentTarget, ExpressionBase valueToAssign)
        {
            InstanceTarget = instanceTarget;
            AssignmentTarget = assignmentTarget;
            ValueToAssign = valueToAssign;
        }

        public override ExpressionBase ReplaceGenericTypeParameters(Dictionary<TypeSymbol, TypeSymbol> typeParameters)
        {
            return new AssignmentExpression(InstanceTarget?.ReplaceGenericTypeParameters(typeParameters), AssignmentTarget, ValueToAssign.ReplaceGenericTypeParameters(typeParameters));
        }
    }
}
