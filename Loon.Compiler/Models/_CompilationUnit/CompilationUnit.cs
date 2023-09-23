using Crater.Shared.Models;
using Loon.Analyzer.Models;
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
        public ResolvedStatement? Statement_Origin { get; private set; } = null;
        public TypedExpression? Expression_Origin { get; private set;} = null;
        public List<CodePiece> CodePieces { get; private set; } = new();

        public CompilationUnit(ResolvedStatement? statement, List<CodePiece> codePieces)
        {
            Statement_Origin = statement;
            CodePieces = codePieces;
        }

        public CompilationUnit(TypedExpression? expression, List<CodePiece> codePieces)
        {
            Expression_Origin = expression;
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
            if (settings.IncludeSourceComments)
            {
                if (Statement_Origin != null) sb.AppendLine($"; {Statement_Origin.RegenerateCode(0).ReplaceLineEndings(Environment.NewLine + ";")}");
                if (Expression_Origin != null) sb.AppendLine($"; {Expression_Origin.RegenerateCode(0).ReplaceLineEndings(Environment.NewLine + ";")}");
            }

            foreach (var codePiece in CodePieces)
            {
                sb.AppendLine(codePiece.ToString().Indent(indentLevel));
            }
            return sb.ToString();
        }
    }
}
