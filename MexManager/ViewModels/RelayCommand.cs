using System;
using System.Windows.Input;

namespace MexManager.ViewModels
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object?>? _execute;
        private readonly Func<object?, bool>? _canExecute;

        public RelayCommand() { }

        public RelayCommand(Action<object?>? execute, Func<object?, bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) =>
            _canExecute == null || _canExecute(parameter);

        public void Execute(object? parameter)
        {
            if (_execute == null)
            {
                throw new InvalidOperationException("Execute action is not set.");
            }

            _execute(parameter);
        }

        public void RaiseCanExecuteChanged() =>
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
