using Crater.Shared.Models;
using Loon.Compiler.Models._Function;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loon.Compiler._Generator
{
    internal static class Generator
    {
        private static Dictionary<CrateType, int> _uniqueIndeces = new Dictionary<CrateType, int>();


        public static string UniqueTypeIdentifier(CrateType type)
        {
            int index = 0;
            if (_uniqueIndeces.TryGetValue(type, out index))
            {
                index++;
            }
            else
            {
                _uniqueIndeces[type] = index;
            }
            return $"{type.Name}_{index}";
        }

        public static string UniqueTypeIdentifier(CrateType type, out string identifier)
        {
            identifier = UniqueTypeIdentifier(type);
            return identifier;
        }

        public static LocalVariable LocalVariable(CrateType type, bool isDiscard = false)
        {
            
            return new LocalVariable(type, UniqueTypeIdentifier(type));
        }

        public static LocalVariable LocalVariable(CrateType type, out LocalVariable local)
        {
            local = LocalVariable(type);
            return local;
        }

        public static LocalVariable LocalVariable(CrateType type, bool isDiscard, out LocalVariable local)
        {
            local = LocalVariable(type, isDiscard);
            
            return local;
        }

        public static LocalVariable LocalVariable(CrateType type, string symbol, out LocalVariable local)
        {
            local = new LocalVariable(type, symbol);
            return local;
        }

        private static int _uniqueLabelIndex = 0;
        public static string Label(string @base) => $"{@base}_{_uniqueLabelIndex++}";
        public static string Label(string @base, out string label)
        {
            label = $"{@base}_{_uniqueLabelIndex++}";
            return label;
        }
    }
}
