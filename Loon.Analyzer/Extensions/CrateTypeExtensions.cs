using Crater.Shared.Models;
using Loon.Analyzer._Analyzer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loon.Analyzer.Extensions
{
    internal static class CrateTypeExtensions
    {
        public static bool IsAssignableFrom(this CrateType assignmentTarget, CrateType valueToAssign)
        {
            return assignmentTarget == valueToAssign || (assignmentTarget.IsReferenceType && valueToAssign == BuiltinTypes.Nullptr) || (assignmentTarget == valueToAssign.CreateInline());
        }

        public static CrateType CreateInline(this CrateType type)
        {
            return new CrateType(type.Name, type.Fields, false);
        }
    }
}
