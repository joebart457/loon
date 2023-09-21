using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loon.Parser.Models.Statements
{
    public class InlineAssemblyStatement: StatementBase
    {
        public string Asm { get; private set; }

        public InlineAssemblyStatement(string asm)
        {
            Asm = asm;
        }
    }
}
