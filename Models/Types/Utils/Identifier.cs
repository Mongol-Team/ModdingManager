using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Types.Utils
{
    public class Identifier : IComparable<Identifier>
    {
        public Identifier(object rawItendifier)
        {
            RawItendifier = rawItendifier;
        }
        public object RawItendifier { get; set; }

        public int ToInt()
        {
            return int.Parse(RawItendifier.ToString());
        }
        public string ToString()
        {
            return RawItendifier.ToString();
        }
        public bool HasValue()
        {
            if (RawItendifier != null)
            {
                return true;
            }
            else return false;
        }

        public int CompareTo(Identifier? other)
        {
            if (other == null) return 1;

            try
            {
                int thisValue = ToInt();
                int otherValue = other.ToInt();
                return thisValue.CompareTo(otherValue);
            }
            catch
            {
                string thisStr = ToString();
                string otherStr = other.ToString();
                return string.Compare(thisStr, otherStr, StringComparison.Ordinal);
            }
        }
        public override bool Equals(object? obj)
        {
            if (obj is Identifier other)
            {
                return CompareTo(other) == 0;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return RawItendifier?.GetHashCode() ?? 0;
        }

        public static bool operator ==(Identifier? left, Identifier? right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left is null || right is null) return false;
            return left.Equals(right);
        }

        public static bool operator !=(Identifier? left, Identifier? right)
        {
            return !(left == right);
        }

    }
}
