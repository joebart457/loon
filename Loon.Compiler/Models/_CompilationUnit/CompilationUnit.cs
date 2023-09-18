using Crater.Shared.Models;
using Loon.Compiler.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loon.Compiler.Models._CompilationUnit
{
    internal class CompilationUnit
    {
        public ResolvedStatement? Origin { get; private set; } = null;
        public List<CodePiece> CodePieces { get; private set; } = new();

        public CompilationUnit(ResolvedStatement? origin, List<CodePiece> codePieces)
        {
            Origin = origin;
            CodePieces = codePieces;
        }

        public CompilationUnit() { }

        public CompilationUnit Append(string code, string? comment = null)
        {
            CodePieces.Add(new CodePiece(code, comment));
            return this;
        }

        public string GenerateAssembly(CompilationSettings settings, int indentLevel = 0)
        {
            var sb = new StringBuilder();
            if (Origin != null) sb.AppendLine($"; {Origin}");
            foreach(var codePiece in CodePieces)
            {
                sb.AppendLine(codePiece.ToString().Indent(indentLevel));
            }
            return sb.ToString();
        }
    }
}
