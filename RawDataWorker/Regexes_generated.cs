using System.Text.RegularExpressions;

namespace RawDataWorker
{
    public static partial class Regexes_generated
    {
        // default chars
        //OpenChar = '{'
        //CloseChar = '}'
        //CommentChar = '#'
        //AssignChar = '='

        private const RegexOptions Common = RegexOptions.CultureInvariant | RegexOptions.Compiled;
        private const RegexOptions Single = Common | RegexOptions.Singleline;
        private const RegexOptions Multi = Common | RegexOptions.Multiline;

        #region Localization
        [GeneratedRegex(@"^(l_\w+:)\n(\ \ [A-Za-z0-9_]+:\ "".+""\n?)+$", Common)]
        public static partial Regex Localization();
        #endregion

        #region Csv
        [GeneratedRegex(@"^([^\;]+\;)+[^;]+$", Common)]
        public static partial Regex CsvFile();

        [GeneratedRegex(@"([^\;\n]+\;?)+", Common)]
        public static partial Regex CsvLine();

        [GeneratedRegex(@"(?<=\;)?[^\;\n]*(?=\;|\n|)", Common)]
        public static partial Regex CsvValue();
        #endregion

        #region Txt

        [GeneratedRegex(@"(25[0-5]|2[0-4]\d|1?\d{1,2})", Common)]
        public static partial Regex HoiColorPart();

        [GeneratedRegex(@"\s*((25[0-5]|2[0-4]\d|1?\d{1,2})\s+){2}(25[0-5]|2[0-4]\d|1?\d{1,2})\s*", Common)]
        public static partial Regex HoiColorContent();

        [GeneratedRegex(@"\w+\s*=\s*((?:(rgb\s*)?{\s*(\d+\s+){2}\d+\s*})|(\"".+\"")|([-\d.]+)|[\w.]+)", Common)]
        public static partial Regex FindVar();

        [GeneratedRegex(@"^\w+\s*=\s*((?:(rgb\s*)?{\s*(\d+\s+){2}\d+\s*})|(\"".+\"")|([-\d.]+)|[\w.]+)$", Common)]
        public static partial Regex Var();

        [GeneratedRegex(@"\w+\s*=\s*((\"".+\"")|[\w.]+|([-\d.]+)|((rgb\s*)?{\s*(\d+\s+){2}\d+\s*}))", Common)]
        public static partial Regex FindStateVar();

        [GeneratedRegex(@"\w+\s*=\s*\{(?:\s*\}|(?!\s*(\d+\s+){2}\d+\s*\})(?=(?>[^{}=]+|\{(?<d>)|\}(?<-d>))+=)(?>[^{}]+|\{(?<d>)|\}(?<-d>))*\}(?(d)(?!)))", Single)]
        public static partial Regex FindBracket();

        [GeneratedRegex(@"^\w+\s*=\s*\{(?:\s*\}|(?!\s*(\d+\s+){2}\d+\s*\})(?=(?>[^{}=]+|\{(?<d>)|\}(?<-d>))+=)(?>[^{}]+|\{(?<d>)|\}(?<-d>))*\}(?(d)(?!)))$", Single)]
        public static partial Regex Bracket();

        [GeneratedRegex(@"^\w+\s*(?==)", Common)]
        public static partial Regex BracketName();

        [GeneratedRegex(@"(?<=\{)(\s*.*)*(?=\}$)", Single)]
        public static partial Regex BracketContent();

        [GeneratedRegex(@"\w+\s*=\s*{(?!\s*(\d+\s+){2}\d+)(?:[^{}=])+?\s*}", Common)]
        public static partial Regex Array();

        [GeneratedRegex(@"^\w+\s*(?==)", Common)]
        public static partial Regex ArrayName();

        [GeneratedRegex(@"(?<=\{)(\s*.*)*(?=\}$)", Single)]
        public static partial Regex ArrayContent();

        [GeneratedRegex(@"(?<=\s)*[^\s]+(?=\s)*", Common)]
        public static partial Regex ArrayElement();

        [GeneratedRegex(@"#.*\s", Common)]
        public static partial Regex FileComment();

        [GeneratedRegex(@"\s*=\s*", Common)]
        public static partial Regex EscapeCharsAroundAssignChar();

        [GeneratedRegex(@"^\s*$\r?\n?", Multi)]
        public static partial Regex EmptyLine();

        #endregion
    }
}
