

namespace Loon.Parser.Models.Statements
{
    public class StatementBase
    {
        public virtual StatementBase ReplaceGenericTypeParameters(Dictionary<TypeSymbol, TypeSymbol> typeParameters)
        {
            return this;
        }
    }
}
