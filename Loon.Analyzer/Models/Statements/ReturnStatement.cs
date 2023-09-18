using Crater.Shared.Models;
using Loon.Parser.Models.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loon.Analyzer.Models
{
    public class ReturnStatement: ResolvedStatement
    {
        public TypedExpression ReturnValue { get; set; }

        public ReturnStatement(TypedExpression returnValue)
        {
            ReturnValue = returnValue;
        }
    }
}
