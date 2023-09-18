using Crater.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loon.Analyzer.Models
{
    public class BlockStatement: ResolvedStatement
    {
        public List<ResolvedStatement> ChildStatements { get; set; }

        public BlockStatement(List<ResolvedStatement> childStatements)
        {
            ChildStatements = childStatements;
        }
    }
}
