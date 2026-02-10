using System.Text;
using System.Text.RegularExpressions;

namespace Application.Extentions
{
    public static class StringExtentions
    {
        public static List<string> ParseQuotedStrings(this string line)
        {
            var results = new List<string>();
            
            var pattern = @"(?<!\\)([""'])(.*?)(?<!\\)\1";

            foreach (Match match in Regex.Matches(line, pattern))
            {
                results.Add(match.Groups[2].Value);
            }

            return results;
        }

        public static string SnakeToPascal(this string input)
        {
            return string.Join("", input
                .Split('_', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => char.ToUpperInvariant(s[0]) + s.Substring(1)));
        }

        public static string SnakeToCamel(this string input)
        {
            var pascal = input.SnakeToPascal();
            return char.ToLowerInvariant(pascal[0]) + pascal.Substring(1);
        }

        public static string CamelToSnake(this string input)
        {
            var sb = new StringBuilder();
            foreach (char c in input)
            {
                if (char.IsUpper(c))
                {
                    if (sb.Length > 0) sb.Append('_');
                    sb.Append(char.ToLowerInvariant(c));
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        public static string PascalToSnake(this string input)
        {
            // Pascal и Camel отличаются только первой буквой,
            // поэтому можно использовать тот же алгоритм
            return CamelToSnake(input);
        }

    }
}
