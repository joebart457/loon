using Loon.Parser.Models.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loon.Parser.Models.Statements
{
    public class ReturnStatement: StatementBase
    {
        public ExpressionBase? ReturnValue { get; set; }

        public ReturnStatement(ExpressionBase? returnValue)
        {
            ReturnValue = returnValue;
        }
    }
}
