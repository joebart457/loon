using Crater.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loon.Parser.Models
{
    public class TypeSymbol
    {
        public bool IsGenericTypeParameter { get; set; } // this indicates whether the current type symbol is used as a generic type parameter, if true GenericTypeArguments must be empty
        public string Name { get; set; } = "";
        public List<TypeSymbol> GenericTypeArguments { get; set; } = new();
        public bool HasGenericTypeArguments => GenericTypeArguments.Any();
        public TypeSymbol(string name, List<TypeSymbol> genericTypeArguments, bool isGenericTypeParameter)
        {
            Name = name;
            GenericTypeArguments = genericTypeArguments;
            IsGenericTypeParameter = isGenericTypeParameter;
            if (isGenericTypeParameter && GenericTypeArguments.Any()) throw new Exception($"type symbol asserts that it is a generic type paramter but contains generic type arguments");
        }

        public string BuildGenericTypeSignature()
        {
            if (!HasGenericTypeArguments)
                return $"{Name}";
            return $"{Name}<{string.Join(", ", GenericTypeArguments.Select(x => x.BuildGenericTypeSignature()))}>";
        }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false ;
            if (obj is  TypeSymbol typeSymbol)
            {
                return Name == typeSymbol.Name && GenericTypeArguments.SequenceEqual(typeSymbol.GenericTypeArguments) && IsGenericTypeParameter == typeSymbol.IsGenericTypeParameter;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override string ToString()
        {
            return BuildGenericTypeSignature();
        }

        public TypeSymbol ReplaceMatchingTypeSymbols(Dictionary<TypeSymbol, TypeSymbol> resolvedTypeArguments)
        {
            if (resolvedTypeArguments.TryGetValue(this, out var replacement)) return replacement;

            return new TypeSymbol(Name, GenericTypeArguments.Select(a => a.ReplaceMatchingTypeSymbols(resolvedTypeArguments)).ToList(), false);
        }

    }
}
