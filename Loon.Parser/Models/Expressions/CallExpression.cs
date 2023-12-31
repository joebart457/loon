﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loon.Parser.Models.Expressions
{
    public class CallExpression: ExpressionBase
    {
        public string CalleeName { get; set; }
        public List<ExpressionBase> Arguments { get; set; }

        public CallExpression(string calleeName, List<ExpressionBase> arguments)
        {
            CalleeName = calleeName;
            Arguments = arguments;
        }

        public override ExpressionBase ReplaceGenericTypeParameters(Dictionary<TypeSymbol, TypeSymbol> typeParameters)
        {
            return new CallExpression(CalleeName, Arguments.Select(a => a.ReplaceGenericTypeParameters(typeParameters)).ToList());
        }
    }
}
