using Crater.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loon.Analyzer.Models
{
    public class AssignmentExpression: TypedExpression
    {
        public TypedExpression? InstanceTarget { get; set; }
        public CrateFieldInfo AssignmentTarget { get; set; }
        public TypedExpression ValueToAssign { get; set; }
        public AssignmentExpression(CrateType type, TypedExpression? instanceTarget, CrateFieldInfo assignmentTarget, TypedExpression valueToAssign)
            :base(type)
        {
            InstanceTarget = instanceTarget;
            AssignmentTarget = assignmentTarget;
            ValueToAssign = valueToAssign;
        }
    }
}
