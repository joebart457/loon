﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loon.Parser.Models.Statements
{
    public class BlockStatement: StatementBase
    {
        public List<StatementBase> ChildStatements { get; set; }

        public BlockStatement(List<StatementBase> childStatements)
        {
            ChildStatements = childStatements;
        }

        public override StatementBase ReplaceGenericTypeParameters(Dictionary<TypeSymbol, TypeSymbol> typeParameters)
        {
            return new BlockStatement(ChildStatements.Select(s => s.ReplaceGenericTypeParameters(typeParameters)).ToList());
        }
    }
}
