using ModdingManagerDataManager.Interfaces;
using ModdingManagerModels.Enums;
using ModdingManagerModels.Interfaces;
using ModdingManagerModels.Types.LocalizationData;
using ModdingManagerModels.Types.LochalizationData;
using System.Text.RegularExpressions;
using Rx = ModdingManagerDataManager.Regexes;

namespace ModdingManagerDataManager.Parsers
{
    public class YmlParser : Parser
    {
        public YmlParser(IParsingPattern _pattern) : base(_pattern) { }

        protected override void Normalize(ref string content)
        {
            content = content.Replace("\r\n", "\n");
            Console.WriteLine(content.Length);
            content = Rx.FileComment.Replace(content, "");
            Console.WriteLine(content.Length);
            content = Rx.EmptyLine.Replace(content, "");
            Console.WriteLine(content.Length);
        }

        protected override IHoiData ParseRealization(string content)
        {
            Normalize(ref content);

            LocalizationFile result = new LocalizationFile();

            MatchCollection localizations = Rx.FindLocalization.Matches(content);
            foreach (Match match in localizations)
            {
                Localization localization = ParseLocalization(match.Value);
                if (localization != null)
                    result.localizations.Add(localization);
            }

            return result;
        }

        private Localization ParseLocalization(string content)
        {
            Localization result = new Localization();

            Language language;
            if (!Enum.TryParse<Language>(Rx.LocalizationLanguage.Match(content).Value, out language)) return null;

            result.Language = language;

            MatchCollection names = Rx.LocalizationVarName.Matches(content);
            MatchCollection values = Rx.LocalizationVarContent.Matches(content);
            MatchCollection keyValuepairs = Rx.LocalizationVar.Matches(content);

            if (names.Count != values.Count) return null;

            for (int q = 0; q < names.Count; q++)
            {
                result.Data.Add(names[q].Value, values[q].Value);
            }

            return result;
        }
    }
}
