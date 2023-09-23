using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loon.Compiler.Models
{
    public enum OutputType
    {
        Exe,
        Dll
    }
    public class CompilationSettings
    {
        public bool NoGC { get; set; }
        public OutputType OutputType { get; set; }
        public string InputFilePath { get; set; } = "";
        public string AssemblyOutputPath { get; set; } = "";
        public string FinalOutputPath { get; set; } = "";
        public bool IncludeSourceComments { get; set; } = true;
    }
}
