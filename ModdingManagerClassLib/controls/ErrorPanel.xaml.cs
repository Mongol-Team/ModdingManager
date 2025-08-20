using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UserControl = System.Windows.Controls.UserControl;

namespace ModdingManager.classes.controls
{
    /// <summary>
    /// Логика взаимодействия для ErrorPanel.xaml
    /// </summary>
    public partial class ErrorPanel : UserControl
    {
     
        // Списки для хранения ошибок
        private List<ErrorBlock> TotalErrors = new List<ErrorBlock>();
        private List<ErrorBlock> WarningErrors = new List<ErrorBlock>();
        private List<ErrorBlock> CriticalErrors = new List<ErrorBlock>();

        // Флаги фильтрации
        private bool showWarnings = true;
        private bool showCritical = true;

        public ErrorPanel()
        {
            InitializeComponent();

            // Инициализация кнопок
            WarnFiler.MouseDown += (s, e) => ToggleWarningFilter();
            CriticalFiler.MouseDown += (s, e) => ToggleCriticalFilter();
        }

        // Метод для добавления ошибки
        public void AddError(ErrorType type, string message, string sourcePath)
        {
            var errorBlock = new ErrorBlock
            {
                ErrorType = type,
                ErrorMessage = message,
                SourcePath = sourcePath,
                Margin = new Thickness(5)
            };

            TotalErrors.Add(errorBlock);

            if (type == ErrorType.Warning)
            {
                WarningErrors.Add(errorBlock);
                WarnCounter.Content = WarningErrors.Count.ToString();
            }
            else
            {
                CriticalErrors.Add(errorBlock);
                CriticalCounter.Content = CriticalErrors.Count.ToString();
            }

            UpdateErrorDisplay();
        }

        // Обновление отображаемых ошибок
        private void UpdateErrorDisplay()
        {
            ErrorDisplayPnl.Children.Clear();

            var errorsToShow = TotalErrors
                .Where(e => (e.ErrorType == ErrorType.Warning && showWarnings) ||
                           (e.ErrorType == ErrorType.Critical && showCritical))
                .ToList();

            foreach (var error in errorsToShow)
            {
                ErrorDisplayPnl.Children.Add(error);
            }
        }

        // Переключение фильтра предупреждений
        private void ToggleWarningFilter()
        {
            showWarnings = !showWarnings;
            WarnFiler.Opacity = showWarnings ? 1.0 : 0.5;
            UpdateErrorDisplay();
        }

        // Переключение фильтра критических ошибок
        private void ToggleCriticalFilter()
        {
            showCritical = !showCritical;
            CriticalFiler.Opacity = showCritical ? 1.0 : 0.5;
            UpdateErrorDisplay();
        }

        // Очистка всех ошибок
        public void ClearErrors()
        {
            TotalErrors.Clear();
            WarningErrors.Clear();
            CriticalErrors.Clear();
            ErrorDisplayPnl.Children.Clear();
            WarnCounter.Content = "0";
            CriticalCounter.Content = "0";
        }
    }
}
