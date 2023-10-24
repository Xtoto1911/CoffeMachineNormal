using CoffeMachineNew.Classes;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace CoffeMachineNew.ViewModel
{
    public class ModelBar : BaseViewModel
    {
        #region Вложенный класс для редактирования файлов сохранений
        private static class ReductionItems<T>
        {
            public static void ItemsToJSON(Collection<T> items, string pathJSON)
            {
                StreamWriter file = File.CreateText(pathJSON);
                foreach (T item in items)
                {
                    string ItemJson = JsonSerializer.Serialize(item, typeof(T));
                    file.WriteLine(ItemJson);
                }
                file.Close();
            }

            public static ObservableCollection<T> GetItemlist(string src)
            {
                string[] lines;
                ObservableCollection<T> items = new();
                if (!File.Exists(src))
                    lines = new string[0];
                else
                    lines = File.ReadAllLines(src);
                foreach (string settingItem in lines)
                    if (settingItem != "")
                        items.Add(JsonSerializer.Deserialize<T>(settingItem));
                return items;
            }

            public static void AddItemToJSON(T item, Collection<T> items)
            {
                if (!items.Contains(item))
                    items.Add(item);
            }

            public static T GetItem(T item, Collection<T> items)
            {
                if (items.Contains(item))
                    return items[items.IndexOf(item)];
                return default;
            }

            public static T GetItem(int idItem, Collection<T> items)
            {
                foreach (T item in items)
                    if ((item as Product).ID == idItem) return item;
                return default;
            }
        }
        #endregion
        #region Хранение и получение данных
        ObservableCollection<Topping> topings;
        public ObservableCollection<Topping> Topings
        {
            set => topings = value;
            get
            {
                topings ??= new();
                var NewItem = (ListCollectionView)CollectionViewSource.GetDefaultView(topings);
                NewItem.NewItemPlaceholderPosition = EditMode ? NewItemPlaceholderPosition.AtBeginning : NewItemPlaceholderPosition.None;
                return topings;
            }
        }

        ObservableCollection<Drink> drinks;
        public ObservableCollection<Drink> Drinks
        {
            set => drinks = value;
            get
            {
                drinks ??= new();
                var NewItem = (ListCollectionView)CollectionViewSource.GetDefaultView(drinks);
                NewItem.NewItemPlaceholderPosition = EditMode ? NewItemPlaceholderPosition.AtBeginning : NewItemPlaceholderPosition.None;
                return drinks;
            }
        }

        string drinkScrPath = string.Empty;
        public string DrinkSrcPath
        {
            get { return drinkScrPath; }
            set
            {
                if (drinkScrPath != value)
                {
                    drinkScrPath = value;
                    Drinks = ReductionItems<Drink>.GetItemlist(DrinkSrcPath);
                }
            }
        }

        string topingScrPath = string.Empty;
        public string TopingSrcPath
        {
            get { return topingScrPath; }
            set
            {
                if (topingScrPath != value)
                {
                    topingScrPath = value;
                    Topings = ReductionItems<Topping>.GetItemlist(TopingSrcPath);
                    var topingView = (ListCollectionView)CollectionViewSource.GetDefaultView(Topings);
                    topingView.NewItemPlaceholderPosition = EditMode ? NewItemPlaceholderPosition.AtBeginning : NewItemPlaceholderPosition.None;
                    OnPropertyChanged(nameof(Topings));
                }
            }
        }

        int _Wallet;
        public int Wallet
        {
            get { return _Wallet; }
            set
            {
                if (_Wallet != value)
                {
                    _Wallet = value;
                    OnPropertyChanged(nameof(Wallet));
                }
            }
        }

        RelayCommand changeAvatar;
        public RelayCommand ChangeAvatar
        {
            get
            {
                return changeAvatar ??=
                    new RelayCommand(obj =>
                    {
                        AvaterPathSelect();
                    });
            }
        }

        public bool Done = true;

        void AvaterPathSelect()
        {
            OpenFileDialog file = new OpenFileDialog();
            file.Filter = "Изображения (*.jpg;*.png)|*.jpg;*.png";
            file.ShowDialog();
            if (file.FileName != "" && file.FileName != SelectedDrink.ImagePath)
                SelectedDrink.ImagePath = file.FileName;
        }
        #endregion
        #region Настройки отображения
        bool _EditMode;
        public bool EditMode //Режим редактирования
        {
            get { return _EditMode; }
            set
            {
                if (_EditMode != value)
                {
                    _EditMode = value;
                    OnPropertyChanged(nameof(EditMode));
                    OnPropertyChanged(nameof(Drinks));
                    OnPropertyChanged(nameof(Topings));
                }
            }
        }

        bool confirmView;
        public bool ConfirmView
        {
            get { return confirmView; }
            set
            {
                if (confirmView != value)
                {
                    confirmView = value;
                    OnPropertyChanged(nameof(ConfirmView));
                }
            }
        }
        bool drinksView = true;
        public bool DrinksView
        {
            get { return drinksView; }
            set
            {
                if (drinksView != value)
                {
                    drinksView = value;
                    OnPropertyChanged(nameof(DrinksView));
                }
            }
        }
        bool topingsView;
        public bool TopingsView
        {
            get { return topingsView; }
            set
            {
                if (topingsView != value)
                {
                    topingsView = value;
                    OnPropertyChanged(nameof(TopingsView));
                    ReductionItems<Topping>.ItemsToJSON(Topings, TopingSrcPath);
                }
            }
        }
        bool progressView;
        public bool ProgressView
        {
            get { return progressView; }
            set
            {
                if (progressView != value)
                {
                    progressView = value;
                    OnPropertyChanged(nameof(ProgressView));
                }
            }
        }
        RelayCommand showDrinks;
        public RelayCommand ShowDrinks
        {
            get
            {
                return showDrinks ??=
                  new RelayCommand(obj =>
                  {
                      DrinksView = true;
                      ProgressView = false;
                      SelectedDrink = null;
                  }, obj => SelectedDrink != null && SelectedDrink.Name != "");
            }
        }

        RelayCommand showTopingsToggle;
        public RelayCommand ShowTopingsToggle
        {
            get
            {
                return showTopingsToggle ??=
                  new RelayCommand(obj =>
                  {
                      TopingsView = !TopingsView;
                      if (!TopingsView)
                          ReductionItems<Topping>.ItemsToJSON(Topings, TopingSrcPath);
                  }, obj => SelectedDrink != null && SelectedDrink.Name != "");//canExecute
            }
        }
        public bool HasTopings
        {
            get { return DrinkTopings.Count > 0; }
        }

        async void ProgressTic(int prcnt)
        {
            OrderProgress = 0;
            OnPropertyChanged(nameof(DenyOrder));
            while (OrderProgress < 100)
            {
                OrderProgress++;

                if (OrderProgress == prcnt)
                {
                    OnOrderCreate();
                    Wallet -= Sum;
                }
                await Task.Delay(100);
            }
            OnPropertyChanged(nameof(DenyOrder));
            OnOrderEnd();
        }
        #endregion
        #region Заказ
        Drink selectedDrink;
        public Drink SelectedDrink
        {
            get { return selectedDrink; }
            set
            {
                if (selectedDrink != value)
                {
                    selectedDrink = value;
                    OnPropertyChanged(nameof(SelectedDrink));
                    if (selectedDrink != null)
                        DrinkSelecting();
                    else
                    {
                        SelectedTopings.Clear();
                        DrinkTopings.Clear();
                        ReductionItems<Drink>.ItemsToJSON(Drinks, DrinkSrcPath);
                    }
                }
            }
        } // Выбраный напиток
        ObservableCollection<Topping> drinkTopings;
        public ObservableCollection<Topping> DrinkTopings
        {
            get => drinkTopings ??= new();
            set => drinkTopings = value;
        } // Доступные топинги
        ObservableCollection<ToppingNode> selectedTopings;
        public ObservableCollection<ToppingNode> SelectedTopings
        {
            get => selectedTopings ??= new();
            set => selectedTopings = value;
        } // ВЫбраные топинги
        #region Команды топингов
        RelayCommand upCount;
        public RelayCommand UpCount
        {
            get
            {
                return upCount ??=
                  new RelayCommand(obj =>
                  {
                      (obj as ToppingNode).Count++;
                      Sum += (obj as ToppingNode).Topping.Price;
                  }, obj => obj != null && (obj as ToppingNode).Count < (obj as ToppingNode).Topping.MaxCnt);//canExecute
            }
        }

        RelayCommand downCount;
        public RelayCommand DownCount
        {
            get
            {
                return downCount ??=
                  new RelayCommand(obj =>
                  {
                      (obj as ToppingNode).Count--;
                      Sum -= (obj as ToppingNode).Topping.Price;
                      if ((obj as ToppingNode).Count < 1)
                          SelectedTopings.Remove(obj as ToppingNode);
                  }, obj => obj != null && (obj as ToppingNode).Count > 0);//canExecute
            }
        }

        RelayCommand addNode;
        public RelayCommand AddNode
        {
            get
            {
                return addNode ??=
                  new RelayCommand(obj =>
                  {
                      SelectedTopings.Add(new(obj as Topping));
                      Sum += (obj as Topping).Price;
                  }, obj =>
                  {
                      if (!(obj is Topping toping)) return false;
                      foreach (ToppingNode node in SelectedTopings)
                          if (node.Topping == toping)
                              return false;
                      return true;
                  });
            }
        }
        #endregion
        int sum; 
        public int Sum
        {
            get { return sum; }
            set
            {
                if (sum != value)
                {
                    sum = value;
                    OnPropertyChanged(nameof(Sum));
                }
            }
        }
        int orderProgress;
        public int OrderProgress
        {
            get { return orderProgress; }
            set
            {
                if (orderProgress != value)
                {
                    orderProgress = value;
                    OnPropertyChanged(nameof(OrderProgress));
                    OnPropertyChanged(nameof(oProgressMessage));
                }
            }
        }
        public string oProgressMessage
        {
            get
            {
                if (OrderProgress < OrderCreatePercent)
                    return $"Подготовка... {OrderProgress}%";
                if (OrderProgress == 100)
                    return "Заберите напиток\nи сдачу";
                return $"Приготовление... {OrderProgress}%";
            }
        }
        public delegate void OrderCreate();
        public event OrderCreate? OnOrderCreate;
        public int OrderCreatePercent { get; set; }
        public delegate void OrderEnd();
        public event OrderEnd? OnOrderEnd;

        RelayCommand createOrder;
        public RelayCommand CreateOrder
        {
            get
            {
                return createOrder ??=
                  new RelayCommand(obj =>
                  {
                      ProgressView = true;
                      ProgressTic(OrderCreatePercent);
                  }, obj => OnOrderCreate != null && Wallet >= Sum && Done);//canExecute
            }
        }

        RelayCommand denyOrder;
        public RelayCommand DenyOrder
        {
            get
            {
                return OrderProgress == 100 ? ShowDrinks : denyOrder ??=
                  new RelayCommand(obj =>
                  {
                      OrderProgress = 100;
                      ProgressView = false;
                  }, obj => (OrderProgress < OrderCreatePercent || OrderProgress == 100) && OnOrderEnd != null);//canExecute
            }
        }
        void DrinkSelecting()
        {
            DrinksView = false;
            Sum = SelectedDrink.Price;
            GetDrinkTopings();
        }
        void GetDrinkTopings()
        {
            DrinkTopings = new();
            foreach (int id in SelectedDrink.Toppings)
                ReductionItems<Topping>.AddItemToJSON(ReductionItems<Topping>.GetItem(id, Topings), DrinkTopings);
            var topingView = (ListCollectionView)CollectionViewSource.GetDefaultView(DrinkTopings);
            topingView.NewItemPlaceholderPosition = EditMode ? NewItemPlaceholderPosition.AtBeginning : NewItemPlaceholderPosition.None;
            OnPropertyChanged(nameof(DrinkTopings));
            OnPropertyChanged(nameof(HasTopings));
        }
        #endregion
        #region Редактирование
        RelayCommand addDrink;
        public RelayCommand AddDrink
        {
            get
            {
                return addDrink ??=
                  new RelayCommand(obj =>
                  {
                      SelectedDrink = new("", 0, "");
                      Drinks.Add(SelectedDrink);
                  }, obj => EditMode);//canExecute
            }
        }
        RelayCommand removeDrink;
        public RelayCommand RemoveDrink
        {
            get
            {
                return !Removing ? ConfirmRemove : removeDrink ??=
                  new RelayCommand(obj =>
                  {
                      Drinks.Remove(SelectedDrink);
                      DrinksView = true;
                      Removing = false;
                  }, obj => true);//canExecute
            }
        }
        RelayCommand addToping;
        public RelayCommand AddToping
        {
            get
            {
                return addToping ??=
                  new RelayCommand(obj =>
                  {
                      Topings.Add(new("", 0, 0));
                  }, obj => EditMode);//canExecute
            }
        }
        RelayCommand removeToping;
        public RelayCommand RemoveToping
        {
            get
            {
                return !Removing ? ConfirmRemove : removeToping ??=
                  new RelayCommand(obj =>
                  {
                      Topings.Remove(obj as Topping);
                      Removing = false;
                      foreach (Drink drink in Drinks)
                          drink.Toppings.Remove((obj as Topping).ID);
                  });
            }
        }
        RelayCommand addDrinkToping;
        public RelayCommand AddDrinkToping
        {
            get
            {
                return addDrinkToping ??=
                  new RelayCommand(obj =>
                  {
                      SelectedDrink.Toppings.Add((obj as Topping).ID);
                      GetDrinkTopings();
                      TopingsView = false;
                  }, obj => obj != null && (obj as Topping).Name != "" && !DrinkTopings.Contains(obj as Topping));//canExecute
            }
        }
        RelayCommand removeDrinkToping;
        public RelayCommand RemoveDrinkToping
        {
            get
            {
                return removeDrinkToping ??=
                  new RelayCommand(obj =>
                  {
                      SelectedDrink.Toppings.Remove((int)obj);
                      GetDrinkTopings();
                      Removing = false;
                  });//canExecute
            }
        }

        bool removing;
        bool Removing
        {
            get { return removing; }
            set
            {
                if (removing != value)
                {
                    removing = value;
                    OnPropertyChanged(nameof(Removing));
                    OnPropertyChanged(nameof(RemoveDrink));
                    OnPropertyChanged(nameof(RemoveToping));
                    OnPropertyChanged(nameof(RemoveDrinkToping));
                }
            }
        }

        RelayCommand confirmRemove;
        public RelayCommand ConfirmRemove
        {
            get
            {
                return confirmRemove ??=
                  new RelayCommand(obj =>
                  {
                      ConfirmView = true;
                  }, obj => true);//canExecute
            }
        }

        RelayCommand removeAnsw;
        public RelayCommand RemoveAnsw
        {
            get
            {
                return removeAnsw ??=
                  new RelayCommand(obj =>
                  {
                      Removing = bool.Parse((string)obj);
                      ConfirmView = false;
                  }, obj => true);//canExecute
            }
        }
        #endregion
    }
}
