using Loon.Shared.Enums;


namespace Crater.Shared.Models
{
    public class CrateParameterInfo
    {
        public string Name { get; set; }
        public CrateType CrateType { get; set; }
        public CrateParameterInfo(string name, CrateType crateType)
        {
            Name = name;
            CrateType = crateType;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            if (obj is CrateParameterInfo parameterInfo)
            {
                return Name == parameterInfo.Name &&
                    CrateType == parameterInfo.CrateType;
            }
            return false;
        }

        public override string ToString()
        {
            return $"{CrateType} {Name}";
        }
    }
    public class CrateFunction
    {
        public string Module { get; set; } = "";
        public CallingConvention CallingConvention { get; set; }
        public bool IsEntry { get; set; }
        public bool IsFFI { get; set; }
        public bool IsExport { get; set; }
        public string Name { get; set; }
        public CrateType ReturnType { get; set; }
        public List<CrateParameterInfo> Parameters { get; set; }
        public List<ResolvedStatement> Body { get; set; }
        private bool _doNotObfuscate = false;
        public CrateFunction(bool isFFI, bool isEntry, CallingConvention callingConvention, string module, string name, CrateType returnType, List<CrateParameterInfo> parameters, List<ResolvedStatement> body, bool isExport, bool doNotObfuscate = false)
        {
            IsFFI = isFFI;
            IsEntry = isEntry;
            CallingConvention = callingConvention;
            Module = module;
            Name = name;
            ReturnType = returnType;
            Parameters = parameters;
            Body = body;
            IsExport = isExport;
            _doNotObfuscate = doNotObfuscate;
        }


        public override string ToString()
        {
            return Name;
        }

        public string GetSignature()
        {
            return $"{Name}({string.Join(",", Parameters)}) : {ReturnType}";
        }

        public string GetDecoratedAssemblyName()
        {
            if (_doNotObfuscate) return Name;
            return $"{Name}!{ReturnType.GetDecoratedAssemblyName()}!{string.Join("!", Parameters.Select(p => p.CrateType.GetDecoratedAssemblyName()))}";
        }
    }

}
