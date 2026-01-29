
using Models.Enums;
using Models.Interfaces;
using Models.Types.LocalizationData;
using Models.Types.LochalizationData;
using System.Text.RegularExpressions;
using Rx = RawDataWorker.Regexes;
using Models.Interfaces.RawDataWorkerInterfaces;

namespace RawDataWorker.Parsers
{
    public class YmlParser : Parser
    {
        public YmlParser(IParsingPattern _pattern) : base(_pattern) { }

        protected override void Normalize(ref string content)
        {
            content = content.Replace("\r\n", "\n");
            content = Rx.FileComment.Replace(content, "");
            content = Rx.EmptyLine.Replace(content, "");
        }

        protected override IHoiData ParseRealization(string content)
        {
            Normalize(ref content);

            LocalizationFile result = new LocalizationFile();

            MatchCollection localizations = Rx.FindLocalization.Matches(content);
            Dictionary<Language, LocalizationBlock> temp = new Dictionary<Language, LocalizationBlock>();
            foreach (Match match in localizations)
            {
                LocalizationBlock localization = ParseLocalization(match.Value);
                if (localization != null)
                {
                    if (!temp.TryAdd(localization.Language, localization))
                    {
                        foreach (var KvP in localization.Data)
                            temp[localization.Language].Data.TryAdd(KvP.Key, KvP.Value);
                    }
                }
            }
            foreach (var Kvp in temp)
            {
                result.Localizations.Add(Kvp.Value);
            }



            return result;
        }

        private LocalizationBlock ParseLocalization(string content)
        {
            LocalizationBlock result = new LocalizationBlock();

            Language language;
            if (!Enum.TryParse<Language>(Rx.LocalizationLanguage.Match(content).Value, out language))
                return null;

            result.Language = language;

            MatchCollection names = Rx.LocalizationVarName.Matches(content);
            MatchCollection values = Rx.LocalizationVarContent.Matches(content);
            //MatchCollection keyValuepairs = Rx.LocalizationVar.Matches(content);

            if (names.Count != values.Count) return null;

            for (int q = 0; q < names.Count; q++)
            {
                result.Data.TryAdd(names[q].Value, values[q].Value);
            }

            return result;
        }
    }
}
