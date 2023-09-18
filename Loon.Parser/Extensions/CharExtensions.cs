using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loon.Parser.Extensions
{
    public static class CharExtensions
    {
        public static bool IsValidModuleChar(this char moduleChar)
        {
            if (char.IsLetterOrDigit(moduleChar)) return true;
            if ("_'/\\:-. ".Contains(moduleChar)) return true;
            return false;
        }
    }
}
