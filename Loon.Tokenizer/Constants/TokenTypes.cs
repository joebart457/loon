using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loon.Tokenizer.Constants
{
    public static class TokenTypes
    {
        public const string Type = "Type";
        public const string Function = "Function";
        public const string LCurly = "LCurly";
        public const string RCurly = "RCurly";
        public const string LParen = "LParen";
        public const string RParen = "RParen";

        public const string Dot = "Dot";
        public const string Colon = "Colon";
        public const string SemiColon = "SemiColon";
        public const string Comma = "Comma";

        public const string InlineAssembly = "InlineAssembly";

        public const string I8 = "I8";
        public const string I16 = "I16";
        public const string I32 = "I32";

        public const string If = "If";
        public const string Else = "Else";
        public const string Return = "Return";
        public const string Var = "Var";
        public const string While = "While";
        public const string For = "For";
        public const string True = "True";
        public const string False = "False";
        public const string Create = "Create";

        public const string Invoke = "Invoke";
        public const string CInvoke = "CInvoke";
        public const string StdCall = "StdCall";
        public const string Inline = "Inline";

        public const string Equal = "Equal";
        public const string DoubleEqual = "DoubleEqual";
        public const string NotEqual = "NotEqual";
        public const string GreaterThan = "GreaterThan";
        public const string GreaterThanEqual = "GreaterThanEqual";
        public const string LessThan = "LessThan";
        public const string LessThanEqual = "LessThanEqual";
        public const string DoubleAmpersand = "DoubleAmpersand";
        public const string DoublePipe = "DoublePipe";
        public const string Not = "Not";
        public const string Minus = "Minus";
        public const string Plus = "Plus";
        public const string ForwardSlash = "ForwardSlash";
        public const string Asterisk = "Asterisk";
        public const string SizeOperator = "SizeOperator";
        public const string Nullptr = "Nullptr";
        public const string Export = "Export";
        public const string Break = "Break";
        public const string Continue = "Continue";
        public const string SizeOf = "SizeOf";

        public const string Entry = "Entry";
        public const string ForeignFunctionInterface = "ForeignFunctionInterface";
        public const string FunctionModule = "FunctionModule";
    }
}
