using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}
