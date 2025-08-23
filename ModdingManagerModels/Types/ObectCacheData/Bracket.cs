using System.Text;

namespace ModdingManagerModels.Types.ObectCacheData
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
        public List<Var> SubVars { get; set; } = new List<Var>();
        public List<Bracket> SubBrackets { get; set; } = new List<Bracket>();
        public string Header { get; set; } = string.Empty;
        public string ToString(int indentLevel = 0)
        {
            var sb = new StringBuilder();
            string indent = new string('\t', indentLevel);
            string innerIndent = new string('\t', indentLevel + 1);

            sb.AppendLine($"{indent}{Header}{AssignSymbol}{OpenChar}");

            // Добавляем переменные
            foreach (var var in SubVars)
            {
                sb.AppendLine($"{innerIndent}{var}");
            }

            // Добавляем обычный контент
            foreach (var line in Content)
            {
                // Исключаем строки, уже представленные как переменные или скобки
                if (!SubVars.Any(v => v.ToString() == line) &&
                    !SubBrackets.Any(b => b.ToString(indentLevel + 1).Contains(line)))
                {
                    sb.AppendLine($"{innerIndent}{line}");
                }
            }

            // Добавляем вложенные скобки
            foreach (var sub in SubBrackets)
            {
                sb.Append(sub.ToString(indentLevel + 1));
            }

            sb.AppendLine($"{indent}{CloseChar}");
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
            SubVars.Add(variable);
        }
        // Возвращает индекс первой скобки по точному совпадению с Header
        public int GetSubBracketIndex(string header)
        {
            if (string.IsNullOrWhiteSpace(header)) return -1;
            return SubBrackets.FindIndex(b => b.Header.Equals(header, StringComparison.OrdinalIgnoreCase));
        }

        // Возвращает индексы всех скобок, содержащих Header
        public List<int> GetAllSubBracketIndices(string header)
        {
            var indices = new List<int>();
            if (string.IsNullOrWhiteSpace(header)) return indices;

            for (int i = 0; i < SubBrackets.Count; i++)
            {
                if (SubBrackets[i].Header.Contains(header, StringComparison.OrdinalIgnoreCase))
                    indices.Add(i);
            }
            return indices;
        }

        // Возвращает индекс первой переменной по точному совпадению с Name
        public int GetSubVarIndex(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return -1;
            return SubVars.FindIndex(v => v.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        // Возвращает индексы всех переменных, содержащих Name
        public List<int> GetAllSubVarIndices(string name)
        {
            var indices = new List<int>();
            if (string.IsNullOrWhiteSpace(name)) return indices;

            for (int i = 0; i < SubVars.Count; i++)
            {
                if (SubVars[i].Name.Contains(name, StringComparison.OrdinalIgnoreCase))
                    indices.Add(i);
            }
            return indices;
        }

        public void AddSubBracket(Bracket subBracket)
        {
            if (subBracket == null || string.IsNullOrWhiteSpace(subBracket.Header)) return;
            SubBrackets.Add(subBracket);
        }

        // Удаляет Var из SubVars по имени
        public void RemoveVarByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return;

            var varToRemove = SubVars.FirstOrDefault(v => v.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (varToRemove != null)
                SubVars.Remove(varToRemove);
        }

        // Заменяет Var в SubVars по имени
        public void ReplaceVarByName(Var newVar)
        {
            if (newVar == null || string.IsNullOrWhiteSpace(newVar.Name)) return;

            var index = SubVars.FindIndex(v => v.Name.Equals(newVar.Name, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
                SubVars[index] = newVar;
        }

        // Удаляет Bracket из SubBrackets по хедеру
        public void RemoveBracketByHeader(string header)
        {
            if (string.IsNullOrWhiteSpace(header)) return;

            var bracketToRemove = SubBrackets.FirstOrDefault(b => b.Header.Equals(header, StringComparison.OrdinalIgnoreCase));
            if (bracketToRemove != null)
                SubBrackets.Remove(bracketToRemove);
        }

        // Заменяет Bracket в SubBrackets по хедеру
        public void ReplaceBracketByHeader(Bracket newBracket)
        {
            if (newBracket == null || string.IsNullOrWhiteSpace(newBracket.Header)) return;

            var index = SubBrackets.FindIndex(b => b.Header.Equals(newBracket.Header, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
                SubBrackets[index] = newBracket;
        }
        // Удаляет все строки из Content, содержащие указанный подстроку
        public void RemoveAllContentContaining(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword)) return;
            Content.RemoveAll(line => line.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        public void ReplaceAllContentContaining(string keyword, string newValue)
        {
            if (string.IsNullOrWhiteSpace(keyword) || string.IsNullOrWhiteSpace(newValue)) return;
            for (int i = 0; i < Content.Count; i++)
            {
                if (Content[i].Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    Content[i] = newValue.Trim();
            }
        }

        // Удаляет все переменные из SubVars по имени (можно использовать частичное совпадение)
        public void RemoveAllVarsByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return;
            SubVars.RemoveAll(v => v.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
        }

        // Заменяет все переменные по имени в SubVars
        public void ReplaceAllVarsByName(string name, Var newVar)
        {
            if (newVar == null || string.IsNullOrWhiteSpace(name)) return;
            for (int i = 0; i < SubVars.Count; i++)
            {
                if (SubVars[i].Name.Contains(name, StringComparison.OrdinalIgnoreCase))
                    SubVars[i] = newVar;
            }
        }
        public void RemoveSubstringFromContentFirst(string substring)
        {
            if (string.IsNullOrWhiteSpace(substring)) return;

            for (int i = 0; i < Content.Count; i++)
            {
                int index = Content[i].IndexOf(substring, StringComparison.OrdinalIgnoreCase);
                if (index >= 0)
                {
                    Content[i] = Content[i].Remove(index, substring.Length);
                    break;
                }
            }
        }

        public void RemoveSubstringFromContentAll(string substring)
        {
            if (string.IsNullOrWhiteSpace(substring)) return;

            for (int i = 0; i < Content.Count; i++)
            {
                while (true)
                {
                    int index = Content[i].IndexOf(substring, StringComparison.OrdinalIgnoreCase);
                    if (index < 0) break;

                    Content[i] = Content[i].Remove(index, substring.Length);
                }
            }
        }
        public void RemoveAllBracketsByHeader(string header)
        {
            if (string.IsNullOrWhiteSpace(header)) return;
            SubBrackets.RemoveAll(b => b.Header.Contains(header, StringComparison.OrdinalIgnoreCase));
        }

        // Заменяет все скобки по хедеру в SubBrackets
        public void ReplaceAllBracketsByHeader(string header, Bracket newBracket)
        {
            if (newBracket == null || string.IsNullOrWhiteSpace(header)) return;
            for (int i = 0; i < SubBrackets.Count; i++)
            {
                if (SubBrackets[i].Header.Contains(header, StringComparison.OrdinalIgnoreCase))
                    SubBrackets[i] = newBracket;
            }
        }

    }
}
