using RawDataWorker.Interfaces;
using ModdingManagerModels.Enums;
using ModdingManagerModels.Interfaces;
using ModdingManagerModels.Types.TableCacheData;
using System.Drawing;
using System.Text.RegularExpressions;
using Rx = RawDataWorker.Regexes;

namespace RawDataWorker.Parsers
{
    /// <exclude/>
    public class CsvParser : Parser
    {
        public CsvParser(IParsingPattern _pattern) : base(_pattern) { }

        protected override IHoiData ParseRealization(string content)
        {
            Normalize(ref content);

            HoiTable result = new HoiTable();

            int expectedValuesPerLine = 0;
            foreach (Type type in pattern.Types)
            {
                if (type.Name == "Color")
                    expectedValuesPerLine += 3;
                else if (type.Name == "Point")
                    expectedValuesPerLine += 2;
                else
                    expectedValuesPerLine++;
            }


            MatchCollection lines = Rx.CsvLine.Matches(content);

            for (int q = 0; q < lines.Count; q++)
            {
                List<object>? line = ParseLine(lines[q].Value, expectedValuesPerLine);
                if (line != null)
                    result.Values.Add(line);
            }

            return result;
        }
        protected override void Normalize(ref string content)
        {
            content = content.Replace("\r\n", "\n");
            if (pattern.Separator == " ")
                content.Replace(" ", ";");
            else
                content.Replace(" ", "");
            content = Rx.FileComment.Replace(content, "");
            content = Rx.EmptyLine.Replace(content, "");
            content = content.Replace($"{pattern.Separator}{pattern.Separator}", $"{pattern.Separator} {pattern.Separator}");
        }
        private List<object>? ParseLine(string content, int expectedValuesPerLine)
        {

            List<object> result = new List<object>();
            var parts = Rx.CsvSeparator.Split(content);

            if (parts.Length < expectedValuesPerLine)
                return null;

            int index = 0;
            foreach (var type in pattern.Types)
            {
                object? value = null;

                switch (type.Name)
                {
                    case "Color":
                        string colorTxt = $"{parts[index]} {parts[index + 1]} {parts[index + 2]}";
                        if (HoiVarsConverter.TryParseColor(colorTxt, out var color))
                        {
                            index += 2;
                            value = color;
                        }

                        break;

                    case "Point":
                        if (Int32.TryParse(parts[index], out var x) && Int32.TryParse(parts[index + 1], out var y))
                        {
                            value = new Point(x, y);
                            index++;
                        }
                        break;

                    case "ProvincesBiome":
                        if (Enum.TryParse<ProvinceTerrain>(parts[index], out var provBiome))
                            value = provBiome;
                        break;

                    case "ProvinceType":
                        if (Enum.TryParse<ProvinceType>(parts[index], out var provType))
                            value = provType;
                        break;

                    case "AdjacencyType":
                        if (Enum.TryParse<AdjacencyType>(parts[index], out var adjaType))
                            value = adjaType;
                        break;

                    case "Int32":
                    case "int":
                        if (HoiVarsConverter.TryParseInteger(parts[index], out var intVal))
                            value = intVal;
                        break;

                    case "Double":
                    case "double":
                        if (HoiVarsConverter.TryParseDouble(parts[index], out var dblVal))
                            value = dblVal;
                        break;

                    case "Boolean":
                    case "bool":
                        if (HoiVarsConverter.TryParseBoolean(parts[index], out var boolVal))
                            value = boolVal;
                        break;

                    case "DateOnly":
                        if (HoiVarsConverter.TryParseDate(parts[index], out var dateVal))
                            value = dateVal;
                        break;

                    case "String":
                    case "string":
                        if (HoiVarsConverter.TryParseString(parts[index], out var strVal))
                            value = strVal;
                        break;

                    case "HoiReference":
                        if (HoiVarsConverter.TryParseHoiReference(parts[index], out var refVal))
                            value = refVal;
                        break;

                    default:
                        value = parts[index];
                        break;
                }
                index++;
                result.Add(value!);
            }
            return result;
        }
    }
}
