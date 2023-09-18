

namespace Crater.Shared.Models
{
    public class CrateFieldInfo
    {
        public string Name { get; set; }
        public CrateType CrateType { get; set; }

        public CrateFieldInfo(string name, CrateType crateType)
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
            if (obj is CrateFieldInfo info)
            {
                return Name == info.Name && CrateType == info.CrateType;
            }
            return false;
        }
    }
    public class CrateType
    {
        public string Name { get; set; }
        public List<CrateFieldInfo> Fields { get; set; }
        public virtual bool IsBuiltin => false;
        public virtual bool IsNumeric => false;
        public virtual bool IsReferenceType { get; private set; } = true;
        public CrateType(string name, List<CrateFieldInfo> fields)
        {
            Name = name;
            Fields = fields;
        }

        public CrateType(string name, List<CrateFieldInfo> fields, bool isReferenceType)
        {
            Name = name;
            Fields = fields;
            IsReferenceType = isReferenceType;
        }

        public CrateFieldInfo? GetField(string name)
        {
            return Fields.FirstOrDefault(f => f.Name == name);
        }

        public override bool Equals(object? obj)
        {
            if (obj is CrateType type)
            {
                return Name == type.Name && IsBuiltin == type.IsBuiltin 
                    && IsNumeric == type.IsNumeric && IsReferenceType == type.IsReferenceType
                    && Fields.SequenceEqual(type.Fields);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override string ToString()
        {
            return Name;
        }

    }
}
