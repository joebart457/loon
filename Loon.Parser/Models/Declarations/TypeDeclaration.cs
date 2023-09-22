using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Loon.Parser.Models.Declarations
{
    public class TypeDeclarationField
    {
        public string FieldName { get; set; }
        public TypeSymbol FieldType { get; set; }
        public bool Inline { get; set; }
        public TypeDeclarationField(string fieldName, TypeSymbol fieldType, bool inline)
        {
            FieldName = fieldName;
            FieldType = fieldType;
            Inline = inline;
        }
    }
    public class TypeDeclaration: DeclarationBase
    {
        public string TypeName { get; set; } = "";
        public List<TypeDeclarationField> Fields { get; set; } = new();

        public TypeDeclaration(string typeName, List<TypeDeclarationField> fields)
        {
            TypeName = typeName;
            Fields = fields;
        }

    }

    public class GenericTypeDeclaration : DeclarationBase
    {
        public string TypeName { get; set; } = "";
        public List<TypeSymbol> GenericTypeParameters { get; set; } = new();
        public List<TypeDeclarationField> Fields { get; set; } = new();

        public GenericTypeDeclaration(string typeName, List<TypeSymbol> genericTypeParameters, List<TypeDeclarationField> fields)
        {
            TypeName = typeName;
            GenericTypeParameters = genericTypeParameters;
            Fields = fields;
        }

        public TypeDeclaration BuildNonGenericType(Dictionary<TypeSymbol, TypeSymbol> resolvedTypeArguments)
        {
            var fields = Fields.Select(f => new TypeDeclarationField(f.FieldName, f.FieldType.ReplaceMatchingTypeSymbols(resolvedTypeArguments), f.Inline)).ToList();
            
            return new TypeDeclaration(TypeName, fields);
        }
    }
}
