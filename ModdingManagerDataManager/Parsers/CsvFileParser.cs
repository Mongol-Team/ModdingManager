using ModdingManagerDataManager.Interfaces;
using ModdingManagerModels.Interfaces;
using ModdingManagerModels.Types.TableCacheData;
using System.Drawing;
using System.Text.RegularExpressions;
using Rx = ModdingManagerDataManager.Regexes;

namespace ModdingManagerDataManager.Parsers
{
    public class CsvFileParser : Parser
    {
        protected override IHoiData ParseRealization(string content, IParsingPattern pattern)
        {
            Normalize(ref content, pattern);

            HoiTable result = new HoiTable();

            int expectedValuesPerLine = 0;
            foreach (Type type in pattern.Types)
            {
                if (type.Name == "Color")
                    expectedValuesPerLine += 3;
                else
                    expectedValuesPerLine++;
            }


            MatchCollection lines = Regex.Matches(content, pattern.Apply(Rx.csvLine));
            for (int q = 0; q < lines.Count; q++)
            {
                result.Values.Add(pattern.Types[q], ParseLine(lines[q].Value, pattern, expectedValuesPerLine));
            }

            return result;
        }
        private static void Normalize(ref string content, IParsingPattern pattern)
        {
            content = content.Replace("\r\n", "\n").Replace(" ", "");

            content = Regex.Replace(content, Rx.funcFileComment, "");
            content = Regex.Replace(content, Rx.emptyLine, "", options: RegexOptions.Multiline);
        }
        private List<object>? ParseLine(string content, IParsingPattern pattern, int expectedValuesPerLine)
        {

            List<object> result = new List<object>();
            List<string> parts = content.Split(pattern.Separator).ToList();

            if (parts.Count != expectedValuesPerLine)
                return null;

            int index = 0;
            foreach (var type in pattern.Types)
            {
                object? value;

                switch (type.Name)
                {
                    case "Color":
                        Color color;
                        string colorTxt = $"{parts[index]} {parts[index + 1]} {parts[index + 2]}"; // reform csv colorTxt to txt colorTxt for working with current converter
                        if (Regex.IsMatch(colorTxt, pattern.Apply(Rx.hoiColorVar)))
                        {
                            HoiVarsConverter.TryParseColor(colorTxt, out color);
                            index += 3;
                            value = color;
                        }
                        return null;
                    case "ProvincesBiomes":

                        return null;



                }

                if (type.Name == "Color")
                {

                }
                else
                {
                    HoiVarsConverter.TryParseAny(parts[index], out value);
                    index++;
                }

            }
            return result;
        }
    }
}
