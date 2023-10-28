using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeMachineNew.Classes
{
    public class ToppingNode : INotifyPropertyChanged
    {
        Topping topping;
        int cnt;
        public ToppingNode(Topping topping)
        {
            Topping = topping;
            cnt = 1;
        }

        public Topping Topping { get; set; }

        public int Count
        {
            get => cnt;
            set
            {
                if (cnt != value)
                {
                    cnt = value;
                    OnPropertyChanged(nameof(Count));
                }
            }
        }


        public int Sum
        {
            get => Count * Topping.Price;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
