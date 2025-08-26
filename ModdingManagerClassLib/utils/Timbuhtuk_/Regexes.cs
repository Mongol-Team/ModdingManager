using ModdingManagerClassLib.Interfaces;

namespace ModdingManagerClassLib.utils.Timbuhtuk_
{
    public struct Regexes
    {

        // default chars
        //OpenChar = '{'
        //CloseChar = '}'
        //CommentChar = '#'
        //AssignChar = '='

        //LOC
        public static string localization = @"^(l_\w+:)\n(\ \ [A-Za-z0-9_]+:\ "".+""\n?)+$";
        //CSV
        public static string csv = @"^([^;]+;)+[^;]+$";



        //TXT

        //use only for extracted Color bracket | { R G B }
        public static string hoiColorPart = @"(25[0-5]|2[0-4]\d|1?\d{1,2})";
        //full version finds only valid colors
        public static string hoiColorVar = @"{\s*(" + hoiColorPart + @"\s+){2}" + hoiColorPart + @"\s*}";
        //trimmed version for detection only
        private static string hoiColorVar_simple = @"{\s*(\d+\s+){2}\d+\s*}";

        public static string findVar = @"\w+\s*=\s*((\"".+\"")|[\w.]+|([-\d.]+)|" + hoiColorVar_simple + @")";
        public static string var = "^" + findVar + "$";

        public static string findBracket = @"\w+\s*=\s*\{(?!\s*(\d+\s+){2}\d+\s*\})(?>[^{}]+|\{(?<d>)|\}(?<-d>))*\}(?(d)(?!))";
        public static string bracket = @"^" + findBracket + @"$";
        //use only for extracted Color bracket | Name={ ... }
        public static string bracketName = @"^\w+(?==)";
        public static string bracketContent = @"(?<=\{)(\s*.*)*(?=\}$)";

        public static string funcFile = @"((([\w])+\s*=\s*(("".+"")|([-\d.]+)|([A-Za-z]+)|({(?!\s*})[^\0]+}))\s*)|(#[^\0]*\s*))*";

        public static string funcFileComment = @"#.*\s";
        public static string escapeCharsAroundAssignChar = @"\s*=\s*";



        // require multiline flag
        public static string emptyLine = @"^\s*$\r?\n?";

        public static string AplyPattern(string regex, IParsingPattern pattern)
        {
            return regex.Replace("{", pattern.OpenChar).Replace("}", pattern.CloseChar).Replace("#", pattern.CommentChar).Replace("=", pattern.AssignChar);
        }
    }
}
