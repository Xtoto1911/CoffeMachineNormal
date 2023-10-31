using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CoffeMachineNew.Classes
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> execute;// Делегат для выполнения команды
        private readonly Func<object, bool> canExecute;// Делегат для определения возможности выполнения команды

        public event EventHandler CanExecuteChanged// Событие, которое происходит при изменении возможности выполнения команды
        {
            add { CommandManager.RequerySuggested += value; }// Подписка на событие CommandManager.RequerySuggested
            remove { CommandManager.RequerySuggested -= value; } // Отписка от события CommandManager.RequerySuggested
        }

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter)//определяет можно ли выполнить команду
        {
            return canExecute == null || canExecute(parameter);
        }

        public void Execute(object parameter)//выполняет команду
        {
            this.execute(parameter);
        }
    }
}
