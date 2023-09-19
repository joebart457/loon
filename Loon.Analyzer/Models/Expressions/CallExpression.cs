using Crater.Shared.Models;
using Loon.Analyzer.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loon.Analyzer.Models
{
    public class CallExpression: TypedExpression
    {
        public CrateFunction CrateFunction { get; set; }
        public List<TypedExpression> Arguments { get; set; }

        public CallExpression(CrateFunction function, List<TypedExpression> arguments)
            :base(function.ReturnType)
        {
            CrateFunction = function;
            Arguments = arguments;
        }

        public override string RegenerateCode(int indentLevel = 0)
        {
            return $"{CrateFunction.Name}({string.Join(", ", Arguments.Select(arg => arg.RegenerateCode(0)))})".Indent(indentLevel);
        }

        
    }
}
