using Models.Enums;
using Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using UserControl = System.Windows.Controls.UserControl;

namespace Controls
{
    /// <summary>
    /// Панель для відображення списку помилок з фільтрацією за типом (Warn / Critical / Fatal)
    /// ОПТИМІЗОВАНА: віртуалізація, батчинг, debouncing
    /// </summary>
    public partial class ErrorPanel : UserControl
    {
        // Колекції для зберігання всіх помилок
        private readonly List<IError> _allErrors = new();
        private readonly List<IError> _warnings = new();
        private readonly List<IError> _criticals = new();
        private readonly List<IError> _fatals = new();

        // Observable колекція для прив'язки до ListBox (автоматична віртуалізація)
        private readonly ObservableCollection<IError> _displayedErrors = new();

        // Флаги фільтрації
        private bool _showWarnings = true;
        private bool _showCriticals = true;
        private bool _showFatals = true;

        // Таймер для відкладеного оновлення UI (debouncing)
        private readonly DispatcherTimer _updateTimer;
        private bool _needsUpdate = false;

        public ErrorPanel()
        {
            InitializeComponent();

            // Прив'язка Observable колекції до ListBox
            ErrorDisplayList.ItemsSource = _displayedErrors;

            // Ініціалізація таймера для батчингу оновлень
            _updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100) // Оновлюємо максимум раз на 100мс
            };
            _updateTimer.Tick += (_, __) =>
            {
                _updateTimer.Stop();
                if (_needsUpdate)
                {
                    PerformUpdateErrorDisplay();
                    _needsUpdate = false;
                }
            };

            // Прив'язка подій до кнопок-фільтрів
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
                    break;

                case ErrorType.Critical:
                    _criticals.Add(error);
                    break;

                case ErrorType.Fatal:
                    _fatals.Add(error);
                    break;
            }

            // Відкладаємо оновлення UI
            ScheduleUpdateErrorDisplay();
        }

        /// <summary>
        /// Додає одразу список помилок (ОПТИМІЗОВАНО: батчинг)
        /// </summary>
        public void AddErrors(IEnumerable<IError> errors)
        {
            if (errors == null) return;

            bool hasNewErrors = false;

            foreach (var error in errors)
            {
                if (error == null) continue;

                _allErrors.Add(error);

                // Розподіл по типах
                switch (error.Type)
                {
                    case ErrorType.Warn:
                        _warnings.Add(error);
                        break;

                    case ErrorType.Critical:
                        _criticals.Add(error);
                        break;

                    case ErrorType.Fatal:
                        _fatals.Add(error);
                        break;
                }

                hasNewErrors = true;
            }

            // Одне оновлення для всіх помилок
            if (hasNewErrors)
            {
                UpdateCounters();
                ScheduleUpdateErrorDisplay();
            }
        }

        /// <summary>
        /// Планує оновлення відображення помилок (debouncing)
        /// </summary>
        private void ScheduleUpdateErrorDisplay()
        {
            _needsUpdate = true;

            if (!_updateTimer.IsEnabled)
            {
                _updateTimer.Start();
            }
        }

        /// <summary>
        /// Оновлює лічильники помилок
        /// </summary>
        private void UpdateCounters()
        {
            if (WarnCounter != null)
                WarnCounter.Content = _warnings.Count.ToString();

            if (CriticalCounter != null)
                CriticalCounter.Content = _criticals.Count.ToString();

            if (FatalCounter != null)
                FatalCounter.Content = _fatals.Count.ToString();
        }

        /// <summary>
        /// Оновлює відображення помилок з урахуванням активних фільтрів
        /// ВІРТУАЛІЗАЦІЯ: Observable колекція + VirtualizingStackPanel автоматично керують видимими елементами
        /// </summary>
        private void PerformUpdateErrorDisplay()
        {
            // Оновлюємо лічильники
            UpdateCounters();

            // Очищаємо Observable колекцію
            _displayedErrors.Clear();

            // Фільтруємо помилки
            var errorsToShow = _allErrors
                .Where(e =>
                    (e.Type == ErrorType.Warn && _showWarnings) ||
                    (e.Type == ErrorType.Critical && _showCriticals) ||
                    (e.Type == ErrorType.Fatal && _showFatals))
                .ToList();

            // Додаємо відфільтровані помилки в Observable колекцію
            // ListBox автоматично віртуалізує відображення - тільки видимі елементи рендеряться
            foreach (var error in errorsToShow)
            {
                _displayedErrors.Add(error);
            }
        }

        private void ToggleWarningFilter()
        {
            _showWarnings = !_showWarnings;
            if (WarnFilter != null)
                WarnFilter.Opacity = _showWarnings ? 1.0 : 0.5;
            ScheduleUpdateErrorDisplay();
        }

        private void ToggleCriticalFilter()
        {
            _showCriticals = !_showCriticals;
            if (CriticalFilter != null)
                CriticalFilter.Opacity = _showCriticals ? 1.0 : 0.5;
            ScheduleUpdateErrorDisplay();
        }

        private void ToggleFatalFilter()
        {
            _showFatals = !_showFatals;
            if (FatalFilter != null)
                FatalFilter.Opacity = _showFatals ? 1.0 : 0.5;
            ScheduleUpdateErrorDisplay();
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
            _displayedErrors.Clear();

            if (WarnCounter != null) WarnCounter.Content = "0";
            if (CriticalCounter != null) CriticalCounter.Content = "0";
            if (FatalCounter != null) FatalCounter.Content = "0";

            _needsUpdate = false;
            _updateTimer.Stop();
        }

        /// <summary>
        /// Кількість усіх помилок (для зовнішнього використання)
        /// </summary>
        public int TotalErrorCount => _allErrors.Count;

        /// <summary>
        /// Кількість відображених помилок після фільтрації
        /// </summary>
        public int DisplayedErrorCount => _displayedErrors.Count;
    }
}