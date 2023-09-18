using Crater.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TokenizerCore.Model;

namespace Loon.Analyzer.Models
{
    public class LiteralExpression: TypedExpression
    {
        public object Value { get; set; }

        public LiteralExpression(CrateType resultingType, object value)
            :base(resultingType)
        {
            Value = value;
        }
    }
}
