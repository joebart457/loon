using Crater.Shared.Models;
using Loon.Analyzer._Analyzer;
using Loon.Compiler.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loon.Compiler.Models
{
    internal class StaticVariable
    {
        public CrateType CrateType { get; private set; }
        public string Symbol { get; private set; }
        public object Value { get; private set; }   
        public StaticVariable(CrateType crateType, string symbol, object value)
        {
            CrateType = crateType;
            Symbol = symbol;
            Value = value;
        }

        public string GenerateAssembly(CompilationSettings settings, int indentLevel = 0)
        {
            var value = GetValueString(Value);
            return $"{Symbol} {GetDataDefinition(CrateType)} {value}".Indent(indentLevel);
        }

        private static string GetDataDefinition(CrateType type)
        {
            if (type == BuiltinTypes.Int32) return "dd";
            if (type == BuiltinTypes.Double) return "dq";

            if (type == BuiltinTypes.String) return "db";
            if (type.IsReferenceType) return "dd";
            throw new Exception($"unable to determine storage class for type {type}");
        }

        private static string GetValueString(object value)
        {
            if (value == null) throw new Exception("unable to determine literal representation for null value");
            if (value is int) return $"{value}";
            if (value is double) return $"{value}";
            if (value is string) return EscapeString($"{value}");
            throw new Exception($"invalid literal: {value}");
        }

        private static string EscapeString(string value)
        {
            if (value.Length == 0) return "0";
            var bytes = Encoding.UTF8.GetBytes(value);
            var strBytes = BitConverter.ToString(bytes);
            return $"{strBytes.Replace("-", "h,")}h,0";
        }
    }
}
