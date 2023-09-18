using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loon.Compiler.Extensions
{
    internal static class StringExtensions
    {
        public static string Indent(this string src, int indentLevel)
        {
            return $"{new string (' ', indentLevel * 4)}{src}";
        }
    }
}
