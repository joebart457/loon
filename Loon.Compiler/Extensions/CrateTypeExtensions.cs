using Crater.Shared.Models;
using Loon.Analyzer._Analyzer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loon.Compiler.Extensions
{
    internal static class CrateTypeExtensions
    {
        public static int GetAssemblySize(this CrateType type)
        {
            int size = 0;
            if (type == BuiltinTypes.Int8) return 1;
            if (type == BuiltinTypes.Int16) return 2;
            if (type == BuiltinTypes.Int32) return 4;
            if (type == BuiltinTypes.Double) return 8;
            if (type == BuiltinTypes.String) return 4;
            foreach (var field in type.Fields)
            {
                if (field.CrateType.IsReferenceType) size += 4;
                else size += field.CrateType.GetAssemblySize();
            }
            return size;
        }
    }
}
