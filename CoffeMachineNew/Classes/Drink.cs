using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeMachineNew.Classes
{
    public class Drink : Product
    {
        private string imagePath;
        public ObservableCollection<int> Toppings { get; set; }
        public Drink(string name, int price, string imagePath) : base(name, price)
        {
            ImagePath = imagePath;
            Toppings = new();
        }

        public Drink()
        {
            ImagePath = "";
        }

        public string ImagePath
        {
            get => imagePath;
            set
            {
                if (imagePath != value)
                {
                    imagePath = value;
                    OnPropertyChanged(nameof(ImagePath));
                }
            }
        }
    }
}
