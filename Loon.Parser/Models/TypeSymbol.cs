using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loon.Parser.Models
{
    public class TypeSymbol
    {
        public string Name { get; set; } = "";

        public TypeSymbol(string name)
        {
            Name = name;
        }
    }
}
