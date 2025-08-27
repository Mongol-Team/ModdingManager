using ModdingManagerDataManager.Interfaces;
using ModdingManagerModels.Enums;
using ModdingManagerModels.Interfaces;
using ModdingManagerModels.Types.ObectCacheData;
using ModdingManagerModels.Types.ObjectCacheData;
using System.Text.RegularExpressions;
using Rx = ModdingManagerDataManager.Regexes;

namespace ModdingManagerDataManager.Parsers
{
    public class FuncFileParser : Parser
    {
        protected override IHoiData ParseRealization(string content, IParsingPattern pattern)
        {

            Normalize(ref content, pattern);

            if (content.Count(c => c.ToString() == pattern.OpenChar) > content.Count(c => c.ToString() == pattern.CloseChar))
                throw new Exception($"Unclosed {pattern.OpenChar}");
            if (content.Count(c => c.ToString() == pattern.OpenChar) < content.Count(c => c.ToString() == pattern.CloseChar))
                throw new Exception($"Unclosed {pattern.CloseChar}");

            HoiFunkFile result = new HoiFunkFile();

            MatchCollection brackets = Regex.Matches(content, pattern.Apply(Rx.findBracket));
            foreach (Match match in brackets)
            {
                result.Brackets.Add(ParseBracket(match.Value, pattern));
            }

            content = Regex.Replace(content, pattern.Apply(Rx.findBracket), "");

            MatchCollection vars = Regex.Matches(content, pattern.Apply(Rx.findVar));
            foreach (Match match in vars)
            {
                result.Vars.Add(ParseVar(match.Value, pattern));
            }

            MatchCollection arrays = Regex.Matches(content, pattern.Apply(Rx.array));
            foreach (Match match in arrays)
            {
                result.Arrays.Add(ParseArray(match.Value, pattern));
            }


            return result;
        }

        private static void Normalize(ref string content, IParsingPattern pattern)
        {
            content = content.Replace("\r\n", "\n");

            content = Regex.Replace(content, Rx.funcFileComment, "");
            content = Regex.Replace(content, Rx.escapeCharsAroundAssignChar, pattern.AssignChar);
            content = Regex.Replace(content, Rx.emptyLine, "", options: RegexOptions.Multiline);
        }
        private static Bracket ParseBracket(string content, IParsingPattern pattern)
        {
            Bracket result = new Bracket();
            if (string.IsNullOrEmpty(content))
                return result;

            result.Name = Regex.Match(content, pattern.Apply(Rx.bracketName)).Value;
            content = Regex.Match(content, pattern.Apply(Rx.bracketContent)).Value;

            MatchCollection brackets = Regex.Matches(content, pattern.Apply(Rx.findBracket));
            foreach (Match match in brackets)
            {
                result.SubBrackets.Add(ParseBracket(match.Value, pattern));
            }

            content = Regex.Replace(content, pattern.Apply(Rx.findBracket), "");

            MatchCollection vars = Regex.Matches(content, pattern.Apply(Rx.findVar));
            foreach (Match match in vars)
            {
                result.SubVars.Add(ParseVar(match.Value, pattern));
            }

            return result;
        }
        private static HoiArray ParseArray(string content, IParsingPattern pattern)
        {
            HoiArray result = new HoiArray();
            if (string.IsNullOrEmpty(content))
                return result;

            result.Name = Regex.Match(content, pattern.Apply(Rx.arrayName)).Value;
            content = Regex.Match(content, pattern.Apply(Rx.arrayContent)).Value;

            MatchCollection values = Regex.Matches(content, pattern.Apply(Rx.arrayElement));

            try
            {
                Type previousType = null;
                foreach (Match match in values)
                {
                    object? value;
                    HoiVarsConverter.TryParseAny(match.Value, out value);

                    if (previousType != null && previousType != value.GetType())
                        throw new ArgumentException($"Array: {content} contain different value types");
                    else
                    {
                        result.PossibleCsType = value.GetType();
                        result.Values.Add(value);
                    }
                }
            }
            catch (Exception)
            {

            }

            return result;
        }
        private static Var? ParseVar(string content, IParsingPattern pattern)
        {
            var parts = content.Split(pattern.AssignChar);
            object? value;
            HoiParsingResult parsingResult = HoiVarsConverter.TryParseAny(parts[1], out value);

            //todo logging for paring errors
            if (parsingResult != HoiParsingResult.Fail)
            {
                Var result = new Var()
                {
                    Name = parts[0],
                    Value = value!,
                    PossibleCsType = (parsingResult == HoiParsingResult.HoiReference ? null : value!.GetType())!,
                    IsHoiReference = parsingResult == HoiParsingResult.HoiReference ? true : false,
                };
                return result;
            }
            return null;
        }

    }
}