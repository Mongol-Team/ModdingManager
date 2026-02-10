using Models.Enums;
using Models.Interfaces;
using Models.Interfaces.RawDataWorkerInterfaces;
using RawDataWorker.Parsers.Errors;
using System.Text.RegularExpressions;

namespace RawDataWorker.Healers
{
    public class TxtHealer : IHealer
    {
        public TxtHealer()
        {
        }
        public List<IError> Errors { get; set; } = new();

        public (string fixedLine, bool success) HealError(IError error, string originalLine)
        {
            Errors.Add(error);

            switch (error)
            {
                case BracketInBracketError _:
                    // удаляем лишние пустые скобки {{ }}
                    string fixed1 = Regex.Replace(originalLine, @"\{\s*\}", "");
                    return (fixed1, true);

                case BracketWithoutAssignmentError _:
                    // вставляем = перед {
                    string fixed2 = Regex.Replace(originalLine, @"(\w+)\s*\{", "$1 = {");
                    return (fixed2, true);

                case UnexpectedContentAfterValueError ue:
                    // удаляем мусор после значения внутри брекета
                    // (нужна более точная логика в зависимости от того, как ты определяешь "мусор")
                    string fixed3 = Regex.Replace(originalLine, @"([^\s=]+)\s+[^={\s][^}]*", "$1");
                    return (fixed3, true);

                case ValueParsingError _:
                case InvalidAssignmentSyntaxError _ when error.Type == ErrorType.Critical:
                    // для Critical можно пробовать подставить дефолтное значение
                    // но это уже сложнее — требует знания контекста
                    // пока оставляем как есть или возвращаем false
                    return (originalLine, false);

                default:
                    // Fatal и Warn не лечим
                    return (originalLine, false);
            }
        }
    }
}