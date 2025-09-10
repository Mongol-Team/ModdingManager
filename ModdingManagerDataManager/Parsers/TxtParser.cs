using ModdingManagerDataManager.Interfaces;
using ModdingManagerModels.Interfaces;
using ModdingManagerModels.Types.ObectCacheData;
using ModdingManagerModels.Types.ObjectCacheData;
using System.Diagnostics;
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

            var (opens, closes) = CountBraces(content, pattern.OpenChar[0], pattern.CloseChar[0]);
            if (opens > closes) throw new Exception($"Unclosed {pattern.OpenChar}");
            if (opens < closes) throw new Exception($"Unclosed {pattern.CloseChar}");

            HoiFuncFile result = new HoiFuncFile();
            content = Rx.FindBracket.Replace(content, m =>
            {
                result.Brackets.Add(ParseBracket(m.Value));
                return string.Empty;
            });

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
            var w = new Stopwatch();
            w.Start();
            content = content.Replace("\r\n", "\n");

            content = Rx.FileComment.Replace(content, "");
            content = Rx.EscapeCharsAroundAssignChar.Replace(content, "=");
            content = Rx.EmptyLine.Replace(content, "");
            w.Stop();
        }
        private Bracket ParseBracket(string content)
        {
            Bracket result = new Bracket();
            if (string.IsNullOrEmpty(content))
                return result;

            result.Name = Rx.BracketName.Match(content).Value;
            content = Rx.BracketContent.Match(content).Value;

            content = Rx.FindBracket.Replace(content, m =>
            {
                result.SubBrackets.Add(ParseBracket(m.Value));
                return string.Empty;
            });



            MatchCollection vars = Rx.FindVar.Matches(content);
            foreach (Match match in vars)
            {
                Var? var = ParseVar(match.Value);
                if (var != null)
                    result.SubVars.Add(var);
            }
            MatchCollection arrays = Rx.Array.Matches(content);
            foreach (Match match in arrays)
            {
                result.Arrays.Add(ParseArray(match.Value));
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
            var w = new Stopwatch();
            w.Start();
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
        private static (int open, int close) CountBraces(string s, char open, char close)
        {
            int o = 0, c = 0;
            foreach (var ch in s) { if (ch == open) o++; else if (ch == close) c++; }
            return (o, c);
        }
    }
}