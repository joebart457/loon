using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loon.Parser.Extensions
{
    public static class StringExtensions
    {
        public static bool IsValidModulePath(this string module)
        {
            if (module.Any(ch => !ch.IsValidModuleChar()) || module.Split('\'').Length > 2) return false;
            return true;
        }
    }
}
