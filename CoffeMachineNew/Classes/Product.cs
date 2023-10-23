using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeMachineNew.Classes
{
    public abstract class Product : INotifyPropertyChanged
    {
        private string name;
        private int price;
        public int ID { get; }
        public Product(string name, int price, int id = 1)
        {
            Name = name;
            Price = price;
            if (id == 1)
            {
                Random random = new Random();
                ID = random.Next(2, 100000);
            }
            else
                ID = id;
        }
        public Product()
        {
            Name = "Название";
            Price = 0;
            Random random = new Random();
            ID = random.Next(2, 100000);
        }
        public string Name
        {
            get => name;
            set
            {
                if (name != value)
                {
                    name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public int Price
        {
            get => price;
            set
            {
                if (price != value)
                {
                    price = value;
                    OnPropertyChanged(nameof(Price));
                }
            }
        }

        public override bool Equals(object? obj)
        {
            return obj is Product @base &&
                   ID == @base.ID;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
