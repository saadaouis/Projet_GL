using System;
using System.Windows.Input;

namespace EasySave.ViewModels
{
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Predicate<T>? _canExecute;

        public RelayCommand(Action<T> execute, Predicate<T>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => 
            _canExecute == null || _canExecute((T)parameter!);

        public void Execute(object? parameter) => 
            _execute((T)parameter!);

        // In Avalonia, we handle CanExecuteChanged differently
        public event EventHandler? CanExecuteChanged;

        protected virtual void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}