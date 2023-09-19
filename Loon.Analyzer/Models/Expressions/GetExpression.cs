using Crater.Shared.Models;
using Loon.Analyzer.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loon.Analyzer.Models
{
    public class GetExpression: TypedExpression
    {
        public TypedExpression InstanceTarget { get; set; }
        public CrateFieldInfo Field { get; set; }
        public GetExpression(CrateType resultingType, TypedExpression instanceTarget, CrateFieldInfo field)
            :base(resultingType)
        {
            InstanceTarget = instanceTarget;
            Field = field;
        }

        public override string RegenerateCode(int indentLevel = 0)
        {
            return $"{InstanceTarget.RegenerateCode(0)}.{Field.Name}".Indent(indentLevel);
        }
    }
}
