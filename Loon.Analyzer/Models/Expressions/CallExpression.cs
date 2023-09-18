using Crater.Shared.Models;
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
        public string CalleeName { get; set; }
        public List<TypedExpression> Arguments { get; set; }

        public CallExpression(CrateFunction function, string calleeName, List<TypedExpression> arguments)
            :base(function.ReturnType)
        {
            CrateFunction = function;
            CalleeName = calleeName;
            Arguments = arguments;
        }
    }
}
