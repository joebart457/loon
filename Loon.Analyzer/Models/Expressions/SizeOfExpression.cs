using Crater.Shared.Models;
using Loon.Analyzer._Analyzer;
using Loon.Analyzer.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loon.Analyzer.Models
{
    public class SizeOfExpression : TypedExpression
    {
        public CrateType TypeArgument { get; private set; }
        public SizeOfExpression(CrateType typeArgument) : base(BuiltinTypes.Int32)
        {
            TypeArgument = typeArgument;
        }

        public override string RegenerateCode(int indentLevel = 0)
        {
            return $"sizeof({TypeArgument})".Indent(indentLevel);
        }
    }
}
