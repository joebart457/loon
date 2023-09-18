﻿using Crater.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TokenizerCore.Interfaces;
using TokenizerCore.Model;

namespace Loon.Analyzer.Models
{
    public class IdentifierExpression: TypedExpression
    {
        public IToken IdentifierSymbol { get; set; }

        public IdentifierExpression(CrateType resultingType, IToken identifierSymbol)
            :base(resultingType)
        {
            IdentifierSymbol = identifierSymbol;
        }
    }
}
