using Crater.Shared.Models;
using Loon.Analyzer.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loon.Analyzer._Analyzer.Comparers
{
    internal class AssignableTypeEqualityComparer : IEqualityComparer<CrateType>
    {
        public bool Equals(CrateType? x, CrateType? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null) || ReferenceEquals(y, null)) return false;
            return x.IsAssignableFrom(y);
        }

        public int GetHashCode([DisallowNull] CrateType obj)
        {
            return obj.GetHashCode();
        }
    }
}
