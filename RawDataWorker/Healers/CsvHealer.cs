using Models.Enums;
using Models.Interfaces;
using Models.Interfaces.RawDataWorkerInterfaces;
using RawDataWorker.Errors;
using System;
using System.Linq;

namespace RawDataWorker.Healers
{
    public class CsvHealer : IHealer
    {
        public List<IError> Errors { get; set; } = new List<IError>();

        private readonly IParsingPattern pattern;

        public CsvHealer(IParsingPattern pattern)
        {
            this.pattern = pattern;
        }

        public (string fixedLine, bool success) HealError(IError error, string originalLine)
        {
            Errors.Add(error);

            switch (error.Type)
            {
                case ErrorType.Warn:
                    return (FixWarn(error, originalLine), true);

                case ErrorType.Critical:
                    var fixedCritical = TryFixCritical(error, originalLine);
                    bool wasFixed = fixedCritical != originalLine;
                    return (wasFixed ? fixedCritical : originalLine, true);

                case ErrorType.Fatal:
                    return (originalLine, false);
                default:
                    return (originalLine, true);
            }
        }

        private string FixWarn(IError error, string line)
        {
            string[] parts = line.Split(pattern.Separator);

            switch (error)
            {
                case ExtraColumnsWarning extra:
                    return string.Join(pattern.Separator, parts.Take(extra.ExpectedCount));

                case MissingRequiredValueWarning missing:
                    int col = missing.ColumnIndex;
                    Type expectedType = pattern.Types[col];

                    string defaultValue = GetDefaultValue(expectedType);
                    parts[col] = defaultValue;
                    return string.Join(pattern.Separator, parts);

                default:
                    return line;
            }
        }

        private string TryFixCritical(IError error, string line)
        {
            if (error is not TypeConversionError conv)
                return line;

            string[] parts = line.Split(pattern.Separator);
            int col = conv.ColumnIndex;
            Type expectedType = pattern.Types[col];

            var (defaultValue, span) = GetDefaultValueWithSpan(expectedType);

            // Проверяем, хватает ли места для замены
            if (col + span > parts.Length)
                return line;

            string[] defaultParts = defaultValue.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < span; i++)
            {
                parts[col + i] = defaultParts[i];
            }

            return string.Join(pattern.Separator, parts);
        }

        private string GetDefaultValue(Type type)
        {
            return GetDefaultValueWithSpan(type).defaultValue;
        }

        private (string defaultValue, int span) GetDefaultValueWithSpan(Type type)
        {
            return type.Name switch
            {
                "Color" => ("0 0 0", 3),
                "Point" => ("0 0", 2),
                "Int32" or "int" => ("0", 1),
                "Double" or "double" => ("0.0", 1),
                "Boolean" or "bool" => ("false", 1),
                "DateOnly" => ("1900.1.1", 1),
                "String" or "string" => ("", 1),
                "HoiReference" => ("none", 1),

                // для всех перечислений по умолчанию
                _ => ("None", 1)
            };
        }
    }
}