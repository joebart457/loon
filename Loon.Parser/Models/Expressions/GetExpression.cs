using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loon.Parser.Models.Expressions
{
    public class GetExpression: ExpressionBase
    {
        public ExpressionBase InstanceTarget { get; set; }
        public string FieldName { get; set; }
        public GetExpression(ExpressionBase instanceTarget, string fieldName)
        {
            InstanceTarget = instanceTarget;
            FieldName = fieldName;
        }
    }
}
