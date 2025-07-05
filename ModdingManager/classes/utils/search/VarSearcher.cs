using ModdingManager.classes.utils.structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModdingManager.classes.utils.search
{
    public class VarSearcher
    {
       
        public static List<Var> ParseAssignments(string content)
        {
            List<Var> varList = new ();
            var assignments = new Dictionary<string, string>();
            string[] lines = content.Split('\n', '\r');

            foreach (string line in lines)
            {
                string trimmed = line.Split('#')[0].Trim();
                if (string.IsNullOrEmpty(trimmed)) continue;

                int eqIndex = trimmed.IndexOf('=');
                if (eqIndex > 0 && !trimmed.Contains('{') && !trimmed.Contains('}'))
                {
                    string key = trimmed.Substring(0, eqIndex).Trim();
                    string value = trimmed.Substring(eqIndex + 1).Trim();
                    Var var = new Var();
                    var.name = key;
                    var.value = value;
                    varList.Add(var);
                }
            }
            return varList;
        }

        public static List<string> ParseQuotedStrings(string content)
        {
            var names = new List<string>();
            int startIndex = 0;

            while ((startIndex = content.IndexOf('\"', startIndex)) != -1)
            {
                int endIndex = content.IndexOf('\"', startIndex + 1);
                if (endIndex == -1) break;

                names.Add(content.Substring(startIndex + 1, endIndex - startIndex - 1));
                startIndex = endIndex + 1;
            }
            return names;
        }

        public static Color ParseColor(string content)
        {
            string[] parts = content.Trim('{', '}').Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3) return Color.Black;

            return Color.FromArgb(
                byte.Parse(parts[0]),
                byte.Parse(parts[1]),
                byte.Parse(parts[2])
            );
        }

        public static bool ParseBool(string value) =>
            value.Equals("yes", StringComparison.OrdinalIgnoreCase);
    }
}
