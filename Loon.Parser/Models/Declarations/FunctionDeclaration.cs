using Loon.Parser.Models.Statements;
using Loon.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Loon.Parser.Models.Declarations
{
    public class FunctionDeclarationParameter
    {
        public string ParameterName { get; set; }
        public TypeSymbol ParameterType { get; set; }

        public FunctionDeclarationParameter(string parameterName, TypeSymbol parameterType)
        {
            ParameterName = parameterName;
            ParameterType = parameterType;
        }
    }
    public class FunctionDeclaration : DeclarationBase
    {
        public bool IsFFI { get; set; } = false;
        public bool IsEntry { get; set; } = false;
        public bool IsExport { get; set; } = false;
        public string Module { get; set; }
        public TypeSymbol ReturnType { get; set; }
        public string FunctionName { get; set; } = "";
        public CallingConvention CallingConvention { get; set; }
        public List<FunctionDeclarationParameter> Parameters { get; set; } = new();
        public List<StatementBase> Body { get; set; } = new();
        public FunctionDeclaration(bool isFFI, bool isEntry, CallingConvention callingConvention, string module, TypeSymbol returnType, string functionName, List<FunctionDeclarationParameter> parameters, List<StatementBase> body, bool isExport)
        {
            IsFFI = isFFI;
            IsEntry = isEntry;
            CallingConvention = callingConvention;
            Module = module;
            ReturnType = returnType;
            FunctionName = functionName;
            Parameters = parameters;
            Body = body;
            IsExport = isExport;
        }
    }
    
    public class GenericFunctionDeclaration : DeclarationBase
    {
        public bool IsFFI { get; set; } = false;
        public bool IsEntry { get; set; } = false;
        public bool IsExport { get; set; } = false;
        public string Module { get; set; }
        public TypeSymbol ReturnType { get; set; }
        public string FunctionName { get; set; } = "";
        public CallingConvention CallingConvention { get; set; }
        public List<TypeSymbol> GenericTypeParameters { get; set; } = new ();

        public List<FunctionDeclarationParameter> Parameters { get; set; } = new();
        public List<StatementBase> Body { get; set; } = new();
        public GenericFunctionDeclaration(bool isFFI, bool isEntry, CallingConvention callingConvention, string module, TypeSymbol returnType, string functionName, List<TypeSymbol> genericTypeParameters, List<FunctionDeclarationParameter> parameters, List<StatementBase> body, bool isExport)
        {
            IsFFI = isFFI;
            IsEntry = isEntry;
            CallingConvention = callingConvention;
            Module = module;
            ReturnType = returnType;
            FunctionName = functionName;
            GenericTypeParameters = genericTypeParameters;
            Parameters = parameters;
            Body = body;
            IsExport = isExport;
        }

        public FunctionDeclaration BuildNonGenericFunctionDeclaration(Dictionary<TypeSymbol, TypeSymbol> resolvedTypeParameters)
        {
            var parameters = Parameters.Select(p => new FunctionDeclarationParameter(p.ParameterName, p.ParameterType.ReplaceMatchingTypeSymbols(resolvedTypeParameters))).ToList();
            var statements = Body.Select(s => s.ReplaceGenericTypeParameters(resolvedTypeParameters)).ToList();
            return new FunctionDeclaration(IsFFI, IsEntry, CallingConvention, Module, ReturnType.ReplaceMatchingTypeSymbols(resolvedTypeParameters), FunctionName, parameters, statements, IsExport);
        }

    }
}
