namespace ModdingManagerModels.Types
{
    public class Var
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public char AssignSymbol { get; set; } = '=';
        public string NamePostfix { get; set; } = string.Empty;
        public string ValuePrefix { get; set; } = string.Empty;
        public string NamePrefix { get; set; } = string.Empty;
        public bool IsValueQuoted { get; set; } = false;
        public VarFormat Format { get; set; } = VarFormat.Normal;
        public enum VarFormat
        {
            Normal,
            Localisation
        }
        public static object GetValueSafe(Var? v) => v?.Value;
        private readonly Dictionary<string, object> _extraProperties = new();

        public void SetValue(object val)
        {
            this.Value = val;
        }
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
            if (this.Format == VarFormat.Normal)
            {
                string namePart = $"{NamePrefix}{Name.Trim('"')}{NamePostfix}";
                string valueCore = $"{ValuePrefix}{Value}";
                string valuePart = IsValueQuoted ? $"\"{valueCore}\"" : valueCore;
                return $"{namePart} {AssignSymbol} {valuePart}";
            }
            else if (this.Format == VarFormat.Localisation)
            {

                string namePart = $"{NamePrefix}{Name.Trim('"')}{NamePostfix}";
                string valueCore = $"{ValuePrefix}{Value}";
                string valuePart = IsValueQuoted ? $"\"{valueCore}\"" : valueCore;
                string res = $" {namePart}{AssignSymbol}{ValuePrefix} {valuePart}";
                return res;
            }
            return string.Empty;
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
        public bool IsStringAreVar(string input)
        {
            if (string.IsNullOrWhiteSpace(input) || IsEmpty())
                return false;

            string formatted = ToString().TrimEnd();

            string escapedFormatted = formatted.Replace("\"", "\\\"");

            return input.Contains(formatted) || input.Contains(escapedFormatted);
        }

    }
}