using System.Text.RegularExpressions;

namespace ModdingManagerClassLib.Extentions
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
    }
}
