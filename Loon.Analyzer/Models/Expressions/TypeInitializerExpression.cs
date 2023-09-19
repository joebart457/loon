using Crater.Shared.Models;
using Loon.Analyzer.Extensions;
using Loon.Parser.Models;
using Loon.Parser.Models.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loon.Analyzer.Models
{
    public class TypeInitializerExpression: TypedExpression
    {
        public TypeInitializerExpression(CrateType crateType)
            :base(crateType)
        {
        }

        public override string RegenerateCode(int indentLevel = 0)
        {
            return $"create {Type.Name}()".Indent(indentLevel); 
        }
    }
}
