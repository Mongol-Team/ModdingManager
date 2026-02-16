using Models.Enums;
using Models.Interfaces;           // IError знаходиться тут
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using UserControl = System.Windows.Controls.UserControl;

namespace Controls
{
    /// <summary>
    /// Панель для відображення списку помилок з фільтрацією за типом (Warn / Critical / Fatal)
    /// </summary>
    public partial class ErrorPanel : UserControl
    {
        // Колекції для зберігання всіх помилок
        private readonly List<IError> _allErrors = new();
        private readonly List<IError> _warnings = new();
        private readonly List<IError> _criticals = new();
        private readonly List<IError> _fatals = new();

        // Флаги фільтрації
        private bool _showWarnings = true;
        private bool _showCriticals = true;
        private bool _showFatals = true;

        public ErrorPanel()
        {
            InitializeComponent();

            // Прив'язка подій до кнопок-фільтрів (якщо у вас вони є в XAML)
            if (WarnFilter != null) WarnFilter.MouseDown += (_, __) => ToggleWarningFilter();
            if (CriticalFilter != null) CriticalFilter.MouseDown += (_, __) => ToggleCriticalFilter();
            if (FatalFilter != null) FatalFilter.MouseDown += (_, __) => ToggleFatalFilter();
        }

        /// <summary>
        /// Додає одну помилку в панель
        /// </summary>
        public void AddError(IError error)
        {
            if (error == null) return;

            _allErrors.Add(error);

            // Розподіл по типах
            switch (error.Type)
            {
                case ErrorType.Warn:
                    _warnings.Add(error);
                    if (WarnCounter != null)
                        WarnCounter.Content = _warnings.Count.ToString();
                    break;

                case ErrorType.Critical:
                    _criticals.Add(error);
                    if (CriticalCounter != null)
                        CriticalCounter.Content = _criticals.Count.ToString();
                    break;

                case ErrorType.Fatal:
                    _fatals.Add(error);
                    if (FatalCounter != null)
                        FatalCounter.Content = _fatals.Count.ToString();
                    break;
            }

            UpdateErrorDisplay();
        }

        /// <summary>
        /// Додає одразу список помилок
        /// </summary>
        public void AddErrors(IEnumerable<IError> errors)
        {
            foreach (var error in errors)
                AddError(error);
        }

        /// <summary>
        /// Оновлює відображення помилок з урахуванням активних фільтрів
        /// </summary>
        private void UpdateErrorDisplay()
        {
            if (ErrorDisplayPnl == null) return;

            ErrorDisplayPnl.Children.Clear();

            var errorsToShow = _allErrors
                .Where(e =>
                    (e.Type == ErrorType.Warn && _showWarnings) ||
                    (e.Type == ErrorType.Critical && _showCriticals) ||
                    (e.Type == ErrorType.Fatal && _showFatals))
                .ToList();

            foreach (var error in errorsToShow)
            {
                var block = new ErrorBlock
                {
                    Error = error,
                    Margin = new Thickness(5, 3, 5, 3)
                };
                ErrorDisplayPnl.Children.Add(block);
            }
        }

        private void ToggleWarningFilter()
        {
            _showWarnings = !_showWarnings;
            if (WarnFilter != null)
                WarnFilter.Opacity = _showWarnings ? 1.0 : 0.5;
            UpdateErrorDisplay();
        }

        private void ToggleCriticalFilter()
        {
            _showCriticals = !_showCriticals;
            if (CriticalFilter != null)
                CriticalFilter.Opacity = _showCriticals ? 1.0 : 0.5;
            UpdateErrorDisplay();
        }

        private void ToggleFatalFilter()
        {
            _showFatals = !_showFatals;
            if (FatalFilter != null)
                FatalFilter.Opacity = _showFatals ? 1.0 : 0.5;
            UpdateErrorDisplay();
        }

        /// <summary>
        /// Повністю очищає всі помилки та скидає лічильники
        /// </summary>
        public void ClearErrors()
        {
            _allErrors.Clear();
            _warnings.Clear();
            _criticals.Clear();
            _fatals.Clear();

            if (ErrorDisplayPnl != null)
                ErrorDisplayPnl.Children.Clear();

            if (WarnCounter != null) WarnCounter.Content = "0";
            if (CriticalCounter != null) CriticalCounter.Content = "0";
            if (FatalCounter != null) FatalCounter.Content = "0";
        }

        /// <summary>
        /// Кількість усіх помилок (для зовнішнього використання)
        /// </summary>
        public int TotalErrorCount => _allErrors.Count;
    }
}