namespace ModdingManagerDataManager
{
    public struct Regexes
    {

        // default chars
        //OpenChar = '{'
        //CloseChar = '}'
        //CommentChar = '#'
        //AssignChar = '='

        #region  LOCALIZATION
        public static string localization = @"^(l_\w+:)\n(\ \ [A-Za-z0-9_]+:\ "".+""\n?)+$";
        #endregion
        #region CSV
        public static string csvFile = @"^([^\;]+\;)+[^;]+$";
        public static string csvLine = @"([^\;\n]+\;?)+";
        public static string csvValue = @"(?<=\;)?[^\;\n]*(?=\;|\n|)";

        #endregion
        #region TXT

        //use only for extracted Color bracket | { R G B }
        public static string hoiColorPart = @"(25[0-5]|2[0-4]\d|1?\d{1,2})";
        //full version finds only valid colors
        public static string hoiColorVar = @"{\s*(" + hoiColorPart + @"\s+){2}" + hoiColorPart + @"\s*}";
        //trimmed version for detection only
        private static string hoiColorVar_simple = @"(rgb\s*)?{\s*(\d+\s+){2}\d+\s*}";



        public static string findVar = @"\w+\s*=\s*((" + hoiColorVar_simple + @")|(\"".+\"")|([-\d.]+)|[\w.]+)";
        public static string var = "^" + findVar + "$";

        public static string findStateVar = @"\w+\s*=\s*((\"".+\"")|[\w.]+|([-\d.]+)|" + hoiColorVar_simple + @")";


        public static string findBracket = @"\w+\s*=\s*\{(?:\s*\}|(?!\s*(\d+\s+){2}\d+\s*\})(?=(?>[^{}=]+|\{(?<d>)|\}(?<-d>))+=)(?>[^{}]+|\{(?<d>)|\}(?<-d>))*\}(?(d)(?!)))";
        public static string bracket = @"^" + findBracket + @"$";
        //use only for extracted & validated bracket | Name={ ... }
        public static string bracketName = @"^\w+\s*(?==)";
        public static string bracketContent = @"(?<=\{)(\s*.*)*(?=\}$)";

        public static string array = @"\w+\s*=\s*{(?!\s*(\d+\s+){2}\d+)(?:[^{}=])+?\s*}";
        //use only for extracted & validated array | Name={ ... }
        public static string arrayName = @"^\w+\s*(?==)";
        public static string arrayContent = @"(?<=\{)(\s*.*)*(?=\}$)";
        public static string arrayElement = @"(?<=\s)*[^\s]+(?=\s)*";

        public static string funcFileComment = @"#.*\s";
        public static string escapeCharsAroundAssignChar = @"\s*=\s*";

        #endregion

        // require multiline flag
        public static string emptyLine = @"^\s*$\r?\n?";
    }
}
