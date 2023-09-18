using Loon.Compiler.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loon.Compiler.Models
{
    internal class AssemblyInclude
    {
        private readonly string _filepath;

        public AssemblyInclude(string filepath)
        {
            _filepath = filepath;
        }

        public string GenerateAssembly(CompilationSettings settings, int indentLevel = 0)
        {
            return $"include '{_filepath}'".Indent(indentLevel);
        }
    }
}
