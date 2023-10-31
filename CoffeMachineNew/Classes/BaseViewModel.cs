using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CoffeMachineNew.Classes
{/// <summary>
/// Метод OnPropertyChanged вызывается при изменении свойства и генерирует событие PropertyChanged, 
/// которое уведомляет подписанные объекты об изменении значения свойства. 
/// В метод передается имя измененного свойства, которое может быть получено автоматически через параметр [CallerMemberName]. 
/// Если в момент вызова события PropertyChanged есть подписчики, то они вызываются с передачей аргументов - объекта, 
/// генерирующего событие (this) и имени измененного свойства.
/// </summary>
    public abstract class BaseViewModel: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string PropertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

        protected virtual bool Set<T>(ref T field, T value, [CallerMemberName] string PropetyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(PropetyName);
            return true;
        }
    }
}
