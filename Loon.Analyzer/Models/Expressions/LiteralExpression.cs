using Crater.Shared.Models;
using Loon.Analyzer.Extensions;
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

        public override string RegenerateCode(int indentLevel = 0)
        {
            return $"{ToValueString(Value)}".Indent(indentLevel);
        }

        private string ToValueString(object value)
        {
            if (value.GetType() == typeof(string)) return $"\"{value}\"".Replace("\r", "\\r").Replace("\n", "\\n");
            if (value.GetType() == typeof(double)) return $"{value}d";
            return $"{value}";
        }
    }
}
