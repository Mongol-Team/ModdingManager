using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModdingManager.classes.utils.types
{
    public class Bracket
    {
        public Bracket() { }
        public char OpenChar { get; set; } = '{';
        public char CloseChar { get; set; } = '}';
        public char CommentSymbol { get; set; } = '#';
        public char AssignSymbol { get; set; } = '=';
        public int StartPosition { get; set; }
        public int EndPosition { get; set; }
        public List<string> Content { get; set; } = new List<string>();
        public List<Var> ContentVars { get; set; } = new List<Var>();
        public List<Bracket> SubBrackets { get; set; } = new List<Bracket>();
        public string Header { get; set; } = string.Empty;
        public string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(Header + '=' + OpenChar);
            foreach (var line in Content)
            {
                sb.AppendLine(line);
            }
            sb.Append(CloseChar);
            return sb.ToString();
        }
        public void AddContent(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return;
            if (line.StartsWith(CommentSymbol.ToString())) return; // Игнорируем комментарии
            Content.Add(line.Trim());
        }
        public void AddVar(Var variable)
        {
            if (variable == null || string.IsNullOrWhiteSpace(variable.Name)) return;
            ContentVars.Add(variable);
            Content.Add(variable.ToString());
        }
        public void AddSubBracket(Bracket subBracket)
        {
            if (subBracket == null || string.IsNullOrWhiteSpace(subBracket.Header)) return;
            SubBrackets.Add(subBracket);
            Content.Add(subBracket.ToString());
        }
    }
}
