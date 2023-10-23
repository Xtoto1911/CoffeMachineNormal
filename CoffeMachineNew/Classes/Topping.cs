using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeMachineNew.Classes
{
    public class Topping : Product
    {
        private int maxCnt = 1;
        public Topping(string name, int price, int maxCnt, int id = 1) : base(name, price, id)
        {
            this.maxCnt = maxCnt;
        }

        public int MaxCnt
        {
            get => maxCnt;
            set
            {
                if (maxCnt != value)
                {
                    maxCnt = value;
                    OnPropertyChanged(nameof(MaxCnt));
                }
            }
        }
    }
}
