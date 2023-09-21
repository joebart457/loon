using Crater.Shared.Models;
using Loon.Analyzer.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loon.Analyzer.Models
{
    public class BreakStatement: ResolvedStatement
    {
        public BreakStatement() { }
        public override string RegenerateCode(int indentLevel = 0)
        {
            return "break".Indent(indentLevel);
        }
    }
}
