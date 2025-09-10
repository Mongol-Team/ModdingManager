using System.Text.RegularExpressions;

namespace ModdingManagerDataManager
{
    public struct Regexes
    {

        // default chars
        //OpenChar = '{'
        //CloseChar = '}'
        //CommentChar = '#'
        //AssignChar = '='

        // =======================
        // COMPILED CACHES
        // =======================

        private static readonly TimeSpan Timeout = TimeSpan.FromMilliseconds(250);

        #region Localization
        private static string localizationLanguage = @"(?<=l_)+[A-Za-z_]+(?=:)";
        public static readonly Regex LocalizationLanguage =
            new Regex(localizationLanguage, RegexOptions.Compiled | RegexOptions.CultureInvariant, Timeout);

        private static string localizationVarName = @"(?<=\ )+[A-Za-z0-9_]+(?=:)";
        public static readonly Regex LocalizationVarName =
            new Regex(localizationVarName, RegexOptions.Compiled | RegexOptions.CultureInvariant, Timeout);

        private static string localizationVarContent = @"(?<="").+(?="")";
        public static readonly Regex LocalizationVarContent =
            new Regex(localizationVarContent, RegexOptions.Compiled | RegexOptions.CultureInvariant, Timeout);

        private static string localization = @"^" + localization + "$";
        public static readonly Regex Localization =
            new Regex(localization, RegexOptions.Compiled | RegexOptions.CultureInvariant, Timeout);

        private static string findLocalization = @"l_\w+:\d*\s+(\ +\w+:\d*\ +"".*""\ *\n?)+";
        public static readonly Regex FindLocalization =
            new Regex(findLocalization, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.NonBacktracking, Timeout);

        private static string localizationVar = @"(?<=\ )+[A-Za-z0-9_]+:.*(?<="").+\""";
        public static readonly Regex LocalizationVar =
            new Regex(localizationVar, RegexOptions.Compiled | RegexOptions.CultureInvariant, Timeout);

        #endregion

        #region Csv
        private static string csvFile = @"^([^\;]+\;)+[^;]+$";
        public static readonly Regex CsvFile =
            new Regex(csvFile, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.NonBacktracking, Timeout);

        private static string csvLine = @"([^\;\n]+\;?)+";
        public static readonly Regex CsvLine =
            new Regex(csvLine, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.NonBacktracking, Timeout);

        // require multiline flag
        private static string csvValue = @"(?<=\;|^)[^\;\n]*(?=\;|\n|)";
        public static readonly Regex CsvValue =
            new Regex(csvValue, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Multiline, Timeout);

        private static string csvSeparator = @";";
        public static readonly Regex CsvSeparator =
            new Regex(csvSeparator, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Multiline | RegexOptions.NonBacktracking, Timeout);

        #endregion

        #region Txt

        // Color
        //trimmed version for detection only
        private static string hoiColorVar_simple = @"(rgb\s*)?{\s*(\d+\s+){2}\d+\s*}";

        //use only for extracted Color bracket | R G B 
        private static string hoiColorPart = @"(25[0-5]|2[0-4]\d|1?\d{1,2})";
        public static readonly Regex HoiColorPart =
            new Regex(hoiColorPart, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.NonBacktracking, Timeout);

        //full version finds only valid colors
        private static string hoiColorContent = @"\s*(" + hoiColorPart + @"\s+){2}" + hoiColorPart + @"\s*";
        public static readonly Regex HoiColorContent =
            new Regex(hoiColorContent, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.NonBacktracking, Timeout);

        // Vars
        private static string findVar = @"\w+\s*=\s*((rgb\s*)?(\"".+\"")|([-\d.]+)|[\w.]+)";
        public static readonly Regex FindVar =
            new Regex(findVar, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.NonBacktracking, Timeout);

        private static string var = "^" + findVar + "$";
        public static readonly Regex Var =
            new Regex(var, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.NonBacktracking, Timeout);

        private static string findStateVar = @"\w+\s*=\s*((\"".+\"")|[\w.]+|([-\d.]+)|" + hoiColorVar_simple + @")";
        public static readonly Regex FindStateVar =
            new Regex(findStateVar, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.NonBacktracking, Timeout);

        // Brackets

        private static string findBracket = @"\w+\s*=\s*\{(?:\s*\}|(?!\s*(\d+\s+){2}\d+\s*\})(?=(?>[^{}=]+|\{(?<d>)|\}(?<-d>))+=)(?>[^{}]+|\{(?<d>)|\}(?<-d>))*\}(?(d)(?!)))";
        public static readonly Regex FindBracket =
            new Regex(findBracket, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline, Timeout);

        private static string bracket = @"^" + findBracket + @"$";
        public static readonly Regex Bracket =
            new Regex(bracket, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline, Timeout);

        //use only for extracted & validated bracket | Name={ ... }
        private static string bracketName = @"^\w+\s*(?==)";
        public static readonly Regex BracketName =
            new Regex(bracketName, RegexOptions.Compiled | RegexOptions.CultureInvariant, Timeout);

        // bracketContent использует .*, значит безопаснее читать как Singleline
        private static string bracketContent = @"(?<=\{)(\s*.*)*(?=\}$)";
        public static readonly Regex BracketContent =
            new Regex(bracketContent, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline, Timeout);



        private static string victoryPoint = @"\w+\s*=\s*{(?!\s*(\d+\s+){2}\d+)(?:[^{}=])+?\s*}";
        public static readonly Regex VictoryPoint =
            new Regex(victoryPoint, RegexOptions.Compiled | RegexOptions.CultureInvariant, Timeout);

        // Arrays


        //use only for extracted & validated array | Name={ ... }
        private static string array = @"\w+\s*=\s*{(?!\s*(\d+\s+){}\d+)(?:[^{}=])+?\s*}";
        public static readonly Regex Array =
            new Regex(array, RegexOptions.Compiled | RegexOptions.CultureInvariant, Timeout);

        private static string arrayName = @"^\w+\s*(?==)";
        public static readonly Regex ArrayName =
            new Regex(arrayName, RegexOptions.Compiled | RegexOptions.CultureInvariant, Timeout);

        private static string arrayContent = @"(?<=\{)(\s*.*)*(?=\}$)";
        public static readonly Regex ArrayContent =
            new Regex(arrayContent, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline, Timeout);

        private static string arrayElement = @"(?<=\s)*[^\s]+(?=\s)*";
        public static readonly Regex ArrayElement =
            new Regex(arrayElement, RegexOptions.Compiled | RegexOptions.CultureInvariant, Timeout);
        #endregion
        // Misc
        private static string fileComment = @"#.*(?=\s)";
        public static readonly Regex FileComment =
            new Regex(fileComment, RegexOptions.Compiled | RegexOptions.CultureInvariant, Timeout);

        private static string escapeCharsAroundAssignChar = @"\s*=\s*";
        public static readonly Regex EscapeCharsAroundAssignChar =
            new Regex(escapeCharsAroundAssignChar, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.NonBacktracking, Timeout);

        // require multiline flag
        private static string emptyLine = @"^\s*$\r?\n?";
        public static readonly Regex EmptyLine =
            new Regex(emptyLine, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Multiline | RegexOptions.NonBacktracking, Timeout);


    }


}
