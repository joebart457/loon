using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loon.Compiler.Models._CompilationUnit
{
    internal class CodePiece
    {
        private readonly string _asm;
        private readonly string? _comment;
        public CodePiece(string asm, string? comment)
        {
            _asm = asm;
            _comment = comment;
        }

        public override string ToString()
        {
            return $"{_asm}{(string.IsNullOrEmpty(_comment) ? "" : $" ; {_comment}")}";
        }
    }
}
