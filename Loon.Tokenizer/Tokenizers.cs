using Loon.Tokenizer.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TokenizerCore;
using TokenizerCore.Model;
using TokenizerCore.Models.Constants;

namespace Loon.Tokenizer
{
    public static class Tokenizers
    {
        private static List<TokenizerRule> _defaultRules => new List<TokenizerRule>()
        {
                    new TokenizerRule(BuiltinTokenTypes.EndOfLineComment, "//"),

                    new TokenizerRule(TokenTypes.Type, "type"),
                    new TokenizerRule(TokenTypes.Function, "fn"),
                    new TokenizerRule(TokenTypes.LCurly, "{"),
                    new TokenizerRule(TokenTypes.RCurly, "}"),
                    new TokenizerRule(TokenTypes.LParen, "("),
                    new TokenizerRule(TokenTypes.RParen, ")"),
                    new TokenizerRule(TokenTypes.Dot, "."),
                    new TokenizerRule(TokenTypes.Colon, ":"),
                    new TokenizerRule(TokenTypes.SemiColon, ";"),
                    new TokenizerRule(TokenTypes.Comma, ","),
                    new TokenizerRule(TokenTypes.If, "if"),
                    new TokenizerRule(TokenTypes.Else, "else"),
                    new TokenizerRule(TokenTypes.Return, "return"),
                    new TokenizerRule(TokenTypes.True, "true"),
                    new TokenizerRule(TokenTypes.False, "false"),
                    new TokenizerRule(TokenTypes.Create, "create"),
                    new TokenizerRule(TokenTypes.Invoke, "__invoke"),
                    new TokenizerRule(TokenTypes.CInvoke, "__cinvoke"),
                    new TokenizerRule(TokenTypes.StdCall, "__stdcall"),
                    new TokenizerRule(TokenTypes.Inline, "__inline"),
                    new TokenizerRule(TokenTypes.Equal, "="),
                    new TokenizerRule(TokenTypes.DoubleEqual, "=="),
                    new TokenizerRule(TokenTypes.NotEqual, "!="),
                    new TokenizerRule(TokenTypes.GreaterThan, ">"),
                    new TokenizerRule(TokenTypes.GreaterThanEqual, ">="),
                    new TokenizerRule(TokenTypes.LessThan, "<"),
                    new TokenizerRule(TokenTypes.LessThanEqual, "<="),
                    new TokenizerRule(TokenTypes.DoubleAmpersand, "&&"),
                    new TokenizerRule(TokenTypes.DoublePipe, "||"),
                    new TokenizerRule(TokenTypes.Not, "!"),
                    new TokenizerRule(TokenTypes.Minus, "-"),
                    new TokenizerRule(TokenTypes.Plus, "+"),
                    new TokenizerRule(TokenTypes.ForwardSlash, "/"),
                    new TokenizerRule(TokenTypes.Asterisk, "*"),
                    new TokenizerRule(TokenTypes.SizeOperator, "$"),
                    new TokenizerRule(TokenTypes.Entry, "entry"),
                    new TokenizerRule(TokenTypes.Var, "var"),
                    new TokenizerRule(TokenTypes.While, "while"),
                    new TokenizerRule(TokenTypes.For, "for"),
                    new TokenizerRule(TokenTypes.Nullptr, "nullptr"),
                    new TokenizerRule(TokenTypes.I8, "i8"),
                    new TokenizerRule(TokenTypes.I16, "i16"),
                    new TokenizerRule(TokenTypes.I32, "i32"),
                    new TokenizerRule(TokenTypes.ForeignFunctionInterface, "ffi"),
                    new TokenizerRule(TokenTypes.Export, "export"),
                    new TokenizerRule(TokenTypes.Continue, "continue"),
                    new TokenizerRule(TokenTypes.Break, "break"),
                    new TokenizerRule(TokenTypes.SizeOf, "sizeof"),
                    new TokenizerRule(BuiltinTokenTypes.String, "\"", enclosingLeft: "\"", enclosingRight: "\""),
                    new TokenizerRule(TokenTypes.FunctionModule, "[", enclosingLeft: "[", enclosingRight: "]"),
                    new TokenizerRule(TokenTypes.InlineAssembly, "__asm {{", enclosingLeft: "__asm {{", enclosingRight: "}}"),
        };
        public static TokenizerSettings DefaultSettings => new TokenizerSettings
        {
            AllowNegatives = true,
            NegativeChar = '-',

        };
        public static TokenizerCore.Tokenizer Default => new TokenizerCore.Tokenizer(_defaultRules, DefaultSettings);
    }
}
