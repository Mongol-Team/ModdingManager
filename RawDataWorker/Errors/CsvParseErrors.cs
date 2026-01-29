using Models.Enums;
using Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RawDataWorker.Errors
{
    // Базовый абстрактный класс для всех ошибок парсинга CSV
    // (можно использовать и напрямую, но обычно лучше конкретные классы)
    public abstract class CsvParseError : IError
    {
        public string Message { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;           // обычно имя файла или относительный путь
        public ErrorType Type { get; set; }
        public bool IsGameError { get; set; } = false;
        public int Line { get; set; }                          // номер строки, если применимо
        protected CsvParseError(ErrorType type)
        {
            Type = type;
        }

        public override string ToString() =>
            $"[{Type}{(Type == ErrorType.Fatal ? ", FATAL" : "")}] {Path} → {Message}";

    }


    /// <summary>
    /// Критическая ошибка — количество значений в строке не соответствует ожидаемому по контракту
    /// </summary>
    public class WrongColumnCountError : CsvParseError
    {
        public int LineNumber { get; }
        public int ActualCount { get; }
        public int ExpectedCount { get; }

        public WrongColumnCountError(
            int lineNumber,
            int actualCount,
            int expectedCount,
            string? filePath = null)
            : base(ErrorType.Fatal)
        {
            LineNumber = lineNumber;
            ActualCount = actualCount;
            ExpectedCount = expectedCount;
            Path = filePath ?? "unknown.csv";

            Message = $"Строка {lineNumber}: неверное количество столбцов. " +
                        $"Ожидалось {expectedCount}, получено {actualCount}";
        }
    }


    /// <summary>
    /// Ошибка преобразования значения в нужный тип данных
    /// </summary>
    public class TypeConversionError : CsvParseError
    {
        public int LineNumber { get; }
        public int ColumnIndex { get; }      // 0-based
        public string ColumnName { get; }    // может быть пустым, если нет заголовков
        public string RawValue { get; }
        public string ExpectedType { get; }

        public TypeConversionError(
            int lineNumber,
            int columnIndex,
            string rawValue,
            string expectedTypeName,
            string? columnName = null,
            string? filePath = null)
            : base(ErrorType.Critical)
        {
            LineNumber = lineNumber;
            ColumnIndex = columnIndex;
            RawValue = rawValue;
            ExpectedType = expectedTypeName;
            ColumnName = columnName ?? $"столбец {columnIndex + 1}";
            Path = filePath ?? "unknown.csv";

            Message = $"Строка {lineNumber}, {ColumnName} (столбец {columnIndex + 1}): " +
                        $"невозможно преобразовать '{rawValue}' в тип {expectedTypeName}";
        }
    }


    /// <summary>
    /// Предупреждение — значение пустое там, где ожидалось обязательное значение
    /// (не критично, но стоит обратить внимание)
    /// </summary>
    public class MissingRequiredValueWarning : CsvParseError
    {
        public int LineNumber { get; }
        public int ColumnIndex { get; }
        public string ColumnName { get; }

        public MissingRequiredValueWarning(
            int lineNumber,
            int columnIndex,
            string? columnName = null,
            string? filePath = null)
            : base(ErrorType.Warn)
        {
            LineNumber = lineNumber;
            ColumnIndex = columnIndex;
            ColumnName = columnName ?? $"столбец {columnIndex + 1}";
            Path = filePath ?? "unknown.csv";

            Message = $"Строка {lineNumber}, {ColumnName}: пустое значение в обязательном поле";
        }
    }


    /// <summary>
    /// Предупреждение — строка содержит лишние столбцы (больше чем ожидалось)
    /// Но при этом основные данные удалось прочитать
    /// </summary>
    public class ExtraColumnsWarning : CsvParseError
    {
        public int LineNumber { get; }
        public int ExpectedCount { get; }
        public int ActualCount { get; }

        public ExtraColumnsWarning(
            int lineNumber,
            int expected,
            int actual,
            string? filePath = null)
            : base(ErrorType.Warn)
        {
            LineNumber = lineNumber;
            ExpectedCount = expected;
            ActualCount = actual;
            Path = filePath ?? "unknown.csv";

            Message = $"Строка {lineNumber}: обнаружены лишние столбцы. " +
                        $"Ожидалось {expected}, найдено {actual}. Лишние данные проигнорированы.";
        }
    }


    /// <summary>
    /// Очень серьёзная структурная ошибка — файл вообще не похож на CSV с ожидаемым разделителем
    /// </summary>
    public class InvalidFormatError : CsvParseError
    {
        public InvalidFormatError(string reason, string? filePath = null)
            : base(ErrorType.Fatal)
        {
            Path = filePath ?? "unknown.csv";
            Message = $"Файл не соответствует ожидаемому формату CSV: {reason}";
        }
    }
}
