using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModdingManager.classes.utils.types
{
    public class Var
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public static object GetValueSafe(Var? v) => v?.Value;
        private readonly Dictionary<string, object> _extraProperties = new();
        
        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(Name) && Value == null;
        }
        public void AddProperty(string propName, object propValue)
        {
            _extraProperties[propName] = propValue;
        }

        public object? GetProperty(string propName)
        {
            _extraProperties.TryGetValue(propName, out var result);
            return result;
        }

        public bool HasProperty(string propName) => _extraProperties.ContainsKey(propName);
        public override string ToString()
        {
            return $"{Name} = {Value}";
        }
        public override bool Equals(object? obj)
        {
            if (obj is Var other)
            {
                return Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase) && 
                       Value?.ToString() == other.Value?.ToString();
            }
            return false;
        }
        public bool EqualsHard(object? obj)
        {
            if (obj is not Var other)
                return false;

            if (!Name.Equals(other.Name, StringComparison.Ordinal))
                return false;

            if (Value == null && other.Value == null)
                return true;

            if (Value == null || other.Value == null)
                return false;

            var thisType = Value.GetType();
            var otherType = other.Value.GetType();
            if (thisType != otherType)
                return false;

            return Value.Equals(other.Value);
        }

    }
}
