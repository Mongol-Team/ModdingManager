using System.Windows.Input;

namespace ModdingManager.WPF
{
    internal class BaseCommand : ICommand
    {
        private Predicate<object>? canExecuteMethod;
        private Action<object>? executeMethod;
        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return canExecuteMethod == null || canExecuteMethod(parameter);
        }
        public BaseCommand(Action<object>? executeMethod, Predicate<object>? canExecuteMethod = null)
        {
            this.canExecuteMethod = canExecuteMethod;
            this.executeMethod = executeMethod;
        }


        public void Execute(object? parameter)
        {
            executeMethod?.Invoke(parameter);
        }
    }
}
