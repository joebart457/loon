using Crater.Shared.Models;
using Loon.Parser.Models.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loon.Analyzer.Models
{
    public class TypedExpression
    {
        public CrateType Type { get; set; }

        public TypedExpression(CrateType type)
        {
            Type = type;
        }
    }
}
