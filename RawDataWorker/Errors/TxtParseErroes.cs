using Models.Enums;
using Models.Interfaces;

namespace RawDataWorker.Parsers.Errors // или другое подходящее пространство имён
{
    public abstract class TxtParseError : IError
    {
        public string Message { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public ErrorType Type { get; set; }
        public bool IsGameError { get; set; }       // новая фича
        public int Line { get; set; }               // номер строки, если удалось определить

        protected TxtParseError(
            string message,
            ErrorType type,
            bool isGameError = false,
            int line = -1,
            string? filePath = null)
        {
            Message = message;
            Type = type;
            IsGameError = isGameError;
            Line = line;
            Path = filePath ?? string.Empty;
        }
    }

    /// <summary>
    /// Несоответствие количества открывающих и закрывающих скобок
    /// </summary>
    public class UnbalancedBracketsError : TxtParseError
    {
        public UnbalancedBracketsError(int openCount, int closeCount, int line = -1, string? filePath = null)
            : base(
                  $"Несбалансированные скобки: {openCount} '{{' и {closeCount} '}}'",
                  ErrorType.Critical,
                  isGameError: false,
                  line,
                  filePath)
        {
        }
    }

    /// <summary>
    /// Некорректный синтаксис использования знака присваивания
    /// (тройное =, = после {, пустое значение и т.д.)
    /// </summary>
    public class InvalidAssignmentSyntaxError : TxtParseError
    {
        public InvalidAssignmentSyntaxError(string details, int line = -1, string? filePath = null)
            : base(
                  $"Некорректный синтаксис присваивания: {details}",
                  ErrorType.Fatal,
                  isGameError: false,
                  line,
                  filePath)
        {
        }

        public static InvalidAssignmentSyntaxError TripleEquals(string fragment, int line)
            => new($"несколько '=' подряд ({fragment})", line);

        public static InvalidAssignmentSyntaxError EqualsAfterOpeningBracket(string fragment, int line)
            => new($"'=' сразу после открывающей скобки ({fragment})", line);

        public static InvalidAssignmentSyntaxError EmptyValueAfterEquals(string fragment, int line)
            => new($"пустое значение после '=' ({fragment})", line);
    }

    /// <summary>
    /// Скобка внутри другой скобки без имени (например: br = { {} })
    /// </summary>
    public class BracketInBracketError : TxtParseError
    {
        public BracketInBracketError(int line = -1, string? filePath = null)
            : base(
                  "Обнаружена безымянная скобка внутри другой скобки ({{ ... }}) — недопустимый синтаксис",
                  ErrorType.Fatal,
                  isGameError: false,
                  line,
                  filePath)
        {
        }
    }

    /// <summary>
    /// Открывающая скобка без знака присваивания перед ней
    /// (например: someName { ... } вместо someName = { ... })
    /// </summary>
    public class BracketWithoutAssignmentError : TxtParseError
    {
        public BracketWithoutAssignmentError(string bracketNameOrFragment, int line = -1, string? filePath = null)
            : base(
                  $"Скобка без знака '=' перед ней: '{bracketNameOrFragment}'",
                  ErrorType.Fatal,
                  isGameError: false,
                  line,
                  filePath)
        {
        }
    }

    /// <summary>
    /// Не удалось распарсить значение переменной
    /// </summary>
    public class ValueParsingError : TxtParseError
    {
        public ValueParsingError(string varName, string rawValue, string attemptedType, int line = -1, string? filePath = null)
            : base(
                  $"Не удалось распарсить значение '{rawValue}' переменной '{varName}' как {attemptedType}",
                  ErrorType.Critical,
                  isGameError: false,
                  line,
                  filePath)
        {
        }
    }

    /// <summary>
    /// Массив содержит элементы разных типов
    /// </summary>
    public class HeterogeneousArrayError : TxtParseError
    {
        public HeterogeneousArrayError(string arrayName, string type1, string type2, int line = -1, string? filePath = null)
            : base(
                  $"Массив '{arrayName}' содержит разные типы: {type1} и {type2}",
                  ErrorType.Warn,
                  isGameError: false,
                  line,
                  filePath)
        {
        }
    }

    /// <summary>
    /// Недопустимое имя (пустое, запрещённые символы и т.д.)
    /// </summary>
    public class InvalidIdentifierError : TxtParseError
    {
        public InvalidIdentifierError(string kind, string invalidName, int line = -1, string? filePath = null)
            : base(
                  $"Недопустимое имя {kind}: '{invalidName}'",
                  ErrorType.Warn,
                  isGameError: false,
                  line,
                  filePath)
        {
        }
    }
    /// <summary>
    /// Лишний мусор после значения
    /// </summary>
    public class UnexpectedContentAfterValueError : TxtParseError
    {
        public UnexpectedContentAfterValueError(string fragment, int line = -1, string? filePath = null)
            : base(
                  $"Лишний контент после значения: {fragment}",
                  ErrorType.Warn,
                  isGameError: false,
                  line,
                  filePath)
        {
        }
    }
    /// <summary>
    /// Пустая секция без содержимого
    /// </summary>
    public class EmptyBracketError : TxtParseError
    {
        public EmptyBracketError(string bracketName, int line = -1, string? filePath = null)
            : base(
                  $"Пустая секция '{bracketName} = {{ }}'",
                  ErrorType.Warn,
                  isGameError: false,
                  line,
                  filePath)
        {
        }
    }
}