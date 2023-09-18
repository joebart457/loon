using Crater.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loon.Compiler.Models
{
    internal class ForeignFunctionReference
    {
        public CrateFunction CrateFunction { get; private set; }
        public string LibraryAlias { get; private set; }
        public string LibraryPath { get; private set; }
        public string FunctionAlias { get; private set; }
        public string FunctionName { get; private set; }
        public ForeignFunctionReference(CrateFunction crateFunction)
        {
            CrateFunction = crateFunction;
            if (crateFunction.Module.Contains("'"))
            {
                var split = crateFunction.Module.Split("'");
                if (split.Length != 2) throw new Exception($"invalid module reference {crateFunction.Module}");
                LibraryPath = split[0];
                FunctionAlias = split[1];
            }else
            {
                LibraryPath = crateFunction.Module;
                FunctionAlias = crateFunction.Name;
            }
            FunctionName = crateFunction.Name;
            LibraryAlias = Path.GetFileNameWithoutExtension(LibraryPath);
        }
    }
}
