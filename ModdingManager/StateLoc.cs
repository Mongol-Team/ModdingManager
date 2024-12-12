using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ModdingManager
{
    public partial class StateLoc : Form
    {
        public StateLoc()
        {
            InitializeComponent();
        }

        public static void GenerateStateAndVictoryPointLocalization(string directoryPath, string victoryPointsList = null)
        {
            // Проверка существования папки
            if (!Directory.Exists(directoryPath))
            {
                Console.WriteLine("Указанная папка не найдена: " + directoryPath);
                return;
            }

            // Получение всех .txt файлов в папке
            string[] files = Directory.GetFiles(directoryPath, "*.txt");

            // Регулярное выражение для поиска строк с "id = "
            Regex idRegex = new Regex(@"id\s*=\s*(\d+)", RegexOptions.Compiled);

            // Регулярное выражение для поиска блока provinces = { ... }
            Regex provincesBlockRegex = new Regex(@"provinces\s*=\s*\{([\s\S]*?)\}", RegexOptions.Compiled);

            // Списки для стейтов и виктори поинтов
            List<string> stateIds = new List<string>();
            List<string> provinceIds = new List<string>();
            List<string> checkedVictoryPoints = new List<string>();
            List<string> missingVictoryPoints = new List<string>();

            // Чтение каждого файла и сбор айдишников стейтов и провинций
            foreach (var file in files)
            {
                string fileContent = File.ReadAllText(file);

                // Поиск ID стейтов
                foreach (Match match in idRegex.Matches(fileContent))
                {
                    stateIds.Add(match.Groups[1].Value);
                }

                // Поиск провинций в блоках provinces = { ... }
                foreach (Match match in provincesBlockRegex.Matches(fileContent))
                {
                    string blockContent = match.Groups[1].Value;
                    // Разбиваем содержимое блока на числа
                    var provinceMatches = Regex.Matches(blockContent, @"\d+");
                    foreach (Match provinceMatch in provinceMatches)
                    {
                        provinceIds.Add(provinceMatch.Value);
                    }
                }
            }

            // Удаляем дубли
            stateIds = stateIds.Distinct().ToList();
            provinceIds = provinceIds.Distinct().ToList();

            // Создаем файл локализации для стейтов
            StringBuilder stateLocalizationBuilder = new StringBuilder();
            stateLocalizationBuilder.AppendLine("l_russian:");
            foreach (var stateId in stateIds)
            {
                stateLocalizationBuilder.AppendLine($" STATE_{stateId}: \"\"");
            }

            string stateLocalizationPath = Path.Combine(directoryPath, "state_localization.yml");
            File.WriteAllText(stateLocalizationPath, stateLocalizationBuilder.ToString(), Encoding.UTF8);
            Console.WriteLine("Файл локализации стейтов создан: " + stateLocalizationPath);

            // Обработка виктори поинтов, если они указаны
            if (!string.IsNullOrEmpty(victoryPointsList))
            {
                var victoryPoints = victoryPointsList.Split(' ').Select(vp => vp.Trim()).ToList();

                foreach (var vp in victoryPoints)
                {
                    if (provinceIds.Contains(vp))
                    {
                        checkedVictoryPoints.Add(vp);
                    }
                    else
                    {
                        missingVictoryPoints.Add(vp);
                    }
                }

                // Показываем MessageBox для отсутствующих виктори поинтов
                if (missingVictoryPoints.Count > 0)
                {
                    MessageBox.Show("Следующие Victory Points не найдены:\n" + string.Join(", ", missingVictoryPoints),
                                    "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                // Создаем файл локализации для виктори поинтов
                StringBuilder vpLocalizationBuilder = new StringBuilder();
                vpLocalizationBuilder.AppendLine("l_russian:");
                foreach (var vp in checkedVictoryPoints)
                {
                    vpLocalizationBuilder.AppendLine($" VICTORY_POINTS_{vp}: \"\"");
                }

                string vpLocalizationPath = Path.Combine(directoryPath, "victory_points_localization.yml");
                File.WriteAllText(vpLocalizationPath, vpLocalizationBuilder.ToString(), Encoding.UTF8);
                Console.WriteLine("Файл локализации виктори поинтов создан: " + vpLocalizationPath);
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                GenerateStateAndVictoryPointLocalization(textBox1.Text, textBox2.Text);
            }
        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                GenerateStateAndVictoryPointLocalization(textBox1.Text, textBox2.Text);
            }
        }
    }
}
