using ModdingManagerDataManager.Interfaces;
using ModdingManagerModels.Interfaces;
using ModdingManagerModels.Types.ObectCacheData;
using System.Text.RegularExpressions;
using Rx = ModdingManagerDataManager.Regexes;




namespace ModdingManagerDataManager.Parsers
{
    public class FunkFileParser : Parser
    {
        protected override IHoiData ParseRealization(string content, IParsingPattern pattern)
        {
            if (!Regex.Match(content, Rx.funcFile).Success)
                return null;

            Normalize(ref content);

            if (content.Count(c => c.ToString() == pattern.OpenChar) > content.Count(c => c.ToString() == pattern.CloseChar))
                throw new Exception($"Unclosed {pattern.OpenChar}");
            if (content.Count(c => c.ToString() == pattern.OpenChar) < content.Count(c => c.ToString() == pattern.CloseChar))
                throw new Exception($"Unclosed {pattern.CloseChar}");

            return ParseFile(content, pattern);
        }
        private static IHoiData ParseFile(string content, IParsingPattern pattern)
        {

            HoiFunkFile result = new HoiFunkFile();

            MatchCollection brackets = Regex.Matches(content, Rx.AplyPattern(Rx.findBracket, pattern));

            foreach (Match match in brackets)
            {
                result.Brackets.Add(ParseBracket(match.Value, pattern));

            }

            content = Regex.Replace(content, Rx.AplyPattern(Rx.findBracket, pattern), "");

            MatchCollection vars = Regex.Matches(content, Rx.AplyPattern(Rx.findVar, pattern));
            foreach (Match match in vars)
            {
                result.Vars.Add(ParseVar(match.Value, pattern));
            }

            return result;
        }
        private static void Normalize(ref string content)
        {
            content = content.Replace("\r\n", "\n");

            content = Regex.Replace(content, Rx.funcFileComment, "");
            content = Regex.Replace(content, Rx.escapeCharsAroundAssignChar, "=");
            content = Regex.Replace(content, Rx.emptyLine, "", options: RegexOptions.Multiline);
        }
        private static Bracket ParseBracket(string content, IParsingPattern pattern)
        {
            Bracket result = new Bracket();
            result.Name = Regex.Match(content, Rx.AplyPattern(Rx.bracketName, pattern)).Value;
            content = Regex.Match(content, Rx.AplyPattern(Rx.bracketContent, pattern)).Value;

            if (string.IsNullOrEmpty(content))
                return result;

            MatchCollection brackets = Regex.Matches(content, Rx.AplyPattern(Rx.findBracket, pattern));
            foreach (Match match in brackets)
            {
                result.SubBrackets.Add(ParseBracket(match.Value, pattern));
            }

            content = Regex.Replace(content, Rx.AplyPattern(Rx.findBracket, pattern), "");

            MatchCollection vars = Regex.Matches(content, Rx.AplyPattern(Rx.findVar, pattern));
            foreach (Match match in vars)
            {
                result.SubVars.Add(ParseVar(match.Value, pattern));
            }

            return result;
        }
        private static Var ParseVar(string content, IParsingPattern pattern)
        {
            var parts = content.Split(pattern.AssignChar);
            object? value;
            HoiVarsConverter.TryParseAny(parts[1], out value);
            Var result = new Var()
            {
                Name = parts[0],
                Value = value,
                PossibleCsType = value.GetType()
            };
            return result;
        }

    }
}