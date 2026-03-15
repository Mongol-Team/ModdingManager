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
        public override string ToString()
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
                string thisValue = ToString();
                string otherValue = other.ToString();
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
            // сравнение по ссылке
            if (ReferenceEquals(left, right)) return true;
            if (left is null || right is null) return false;
            return left.Equals(right);
        }

        public static bool operator !=(Identifier? left, Identifier? right) => !(left == right);


        public static bool operator ==(string? left, Identifier? right)
        {
            // если сравниваем с null → только по ссылке
            if (left is null) return right is null;
            if (right is null) return false;
            return left == right.RawItendifier?.ToString();
        }

        public static bool operator !=(string? left, Identifier? right) => !(left == right);


    }
}

