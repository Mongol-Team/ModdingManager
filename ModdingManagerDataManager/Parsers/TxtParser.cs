using ModdingManagerDataManager.Interfaces;
using ModdingManagerModels.Interfaces;
using ModdingManagerModels.Types.ObectCacheData;
using ModdingManagerModels.Types.ObjectCacheData;
using System.Text.RegularExpressions;
using Rx = ModdingManagerDataManager.Regexes;

namespace ModdingManagerDataManager.Parsers
{
    public class TxtParser : Parser
    {
        public TxtParser(IParsingPattern _pattern) : base(_pattern) { }
        protected override IHoiData ParseRealization(string content)
        {

            Normalize(ref content);

            if (content.Count(c => c.ToString() == pattern.OpenChar) > content.Count(c => c.ToString() == pattern.CloseChar))
                throw new Exception($"Unclosed {pattern.OpenChar}");
            if (content.Count(c => c.ToString() == pattern.OpenChar) < content.Count(c => c.ToString() == pattern.CloseChar))
                throw new Exception($"Unclosed {pattern.CloseChar}");

            HoiFunkFile result = new HoiFunkFile();

            MatchCollection brackets = Rx.FindBracket.Matches(content);
            foreach (Match match in brackets)
            {
                result.Brackets.Add(ParseBracket(match.Value));
            }

            content = Rx.FindBracket.Replace(content, "");

            MatchCollection vars = Rx.FindVar.Matches(content);
            foreach (Match match in vars)
            {
                Var? var = ParseVar(match.Value);
                if (var != null)
                    result.Vars.Add(var);
            }

            MatchCollection arrays = Rx.Array.Matches(content);
            foreach (Match match in arrays)
            {
                result.Arrays.Add(ParseArray(match.Value));
            }


            return result;
        }

        protected override void Normalize(ref string content)
        {
            content = content.Replace("\r\n", "\n");

            content = Rx.FileComment.Replace(content, "");
            content = Rx.EscapeCharsAroundAssignChar.Replace(content, "");
            content = Rx.EmptyLine.Replace(content, "");
        }
        private Bracket ParseBracket(string content)
        {
            Bracket result = new Bracket();
            if (string.IsNullOrEmpty(content))
                return result;

            result.Name = Rx.BracketName.Match(content).Value;
            content = Rx.BracketContent.Match(content).Value;

            MatchCollection brackets = Rx.FindBracket.Matches(content);
            foreach (Match match in brackets)
            {
                result.SubBrackets.Add(ParseBracket(match.Value));
            }

            content = Rx.FindBracket.Replace(content, "");

            MatchCollection vars = Rx.FindVar.Matches(content);
            foreach (Match match in vars)
            {
                Var? var = ParseVar(match.Value);
                if (var != null)
                    result.SubVars.Add(var);
            }

            return result;
        }
        private HoiArray ParseArray(string content)
        {
            HoiArray result = new HoiArray();
            if (string.IsNullOrEmpty(content))
                return result;

            result.Name = Rx.ArrayName.Match(content).Value;
            content = Rx.ArrayContent.Match(content).Value;

            MatchCollection values = Rx.ArrayElement.Matches(content);

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
        private Var? ParseVar(string content)
        {
            var parts = content.Split(pattern.AssignChar);
            object? value;
            bool parsingResult = HoiVarsConverter.TryParseAny(parts[1], out value);

            //todo logging for parsing errors
            if (parsingResult)
            {
                Var result = new Var()
                {
                    Name = parts[0],
                    Value = value,
                    PossibleCsType = (parsingResult == false ? null : value!.GetType())!,
                };
                return result;
            }
            return null;
        }

    }
}