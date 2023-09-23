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

        public static string Decorate(this string src)
        {
            return $".!{src}";
        }

        public static string CapitalizeFirst(this string src)
        {
            if (string.IsNullOrWhiteSpace(src)) return src;
            return $"{src.Substring(0, 1).ToUpper()}{src.Substring(1)}";
        }

        public static StringBuilder AppendLineIfNotEmpty(this StringBuilder sb, string str)
        {
            if (string.IsNullOrEmpty(str)) return sb;
            sb.AppendLine(str);
            return sb;
        }
    }
}
