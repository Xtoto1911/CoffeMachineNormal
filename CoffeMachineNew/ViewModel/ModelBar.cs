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

            public static void AddItemToJSON(Collection<T> items, T item)
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
            get
            {
                topings ??= new();
                var NewItem = (ListCollectionView)CollectionViewSource.GetDefaultView(topings);
                NewItem.NewItemPlaceholderPosition = EditMode ? NewItemPlaceholderPosition.AtBeginning : NewItemPlaceholderPosition.None;
                return topings;
            }
            set => topings = value;
        }
        ObservableCollection<Drink> drinks;
        public ObservableCollection<Drink> Drinks
        {
            get
            {
                drinks ??= new();
                var NewItem = (ListCollectionView)CollectionViewSource.GetDefaultView(drinks);
                NewItem.NewItemPlaceholderPosition = EditMode ? NewItemPlaceholderPosition.AtBeginning : NewItemPlaceholderPosition.None;
                return drinks;
            }
            set => drinks = value;
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

        void AvaterPathSelect()
        {
            OpenFileDialog file = new OpenFileDialog();
            file.ShowDialog();
            if (file.FileName != "" && file.FileName != SelectedDrink.ImagePath)
                SelectedDrink.ImagePath = file.FileName;
        }

        #endregion
        #region Настройки отображения
        bool editMode = false;
        public bool EditMode //AdminPanel
        {
            get { return editMode; }
            set
            {
                if (editMode != value)
                {
                    editMode = value;
                    OnPropertyChanged(nameof(EditMode));
                    OnPropertyChanged(nameof(Drinks));
                    OnPropertyChanged(nameof(Topings));
                }
            }
        }
        bool drinksView = true;
        public bool DrinksView//MainPanel
        {
            get { return drinksView; }
            set
            {
                if (drinksView != value)
                {
                    drinksView = value;
                    OnPropertyChanged(nameof(DrinksView));
                    OnPropertyChanged(nameof(OrderSetup));
                    OnPropertyChanged(nameof(DrinkEdit));
                }
            }
        }
        bool topingView = false;
        public bool TopingView
        {
            get { return topingView; }
            set
            {
                if (topingView != value)
                {
                    topingView = value;
                    OnPropertyChanged(nameof(TopingView));
                    ReductionItems<Topping>.ItemsToJSON(Topings, TopingSrcPath);
                }
            }
        }

        public bool OrderSetup//OrderPanel
        {
            get { return !editMode && !drinksView; }
        }

        public bool DrinkEdit//isEditInterectionPanel
        {
            get { return editMode && !drinksView; }
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
                      TopingView = !TopingView;
                      if (!TopingView)
                          ReductionItems<Topping>.ItemsToJSON(Topings, TopingSrcPath);
                  }, obj => SelectedDrink != null && SelectedDrink.Name != "");//canExecute
            }
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
        public RelayCommand DeCount
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
        int sum; // Сумма заказа
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

        
        public delegate void OrderCreate();
        public event OrderCreate? OnOrderCreate;
        public int OrderCreatePercent { get; set; }
        public delegate void OrderEnd();
        public event OrderEnd? OnOrderEnd;

        

        
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
                ReductionItems<Topping>.AddItemToJSON(DrinkTopings, ReductionItems<Topping>.GetItem(id, Topings));
            var topingView = (ListCollectionView)CollectionViewSource.GetDefaultView(DrinkTopings);
            topingView.NewItemPlaceholderPosition = EditMode ? NewItemPlaceholderPosition.AtBeginning : NewItemPlaceholderPosition.None;
            OnPropertyChanged(nameof(DrinkTopings));
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
                      SelectedDrink = new("",0,"");
                      Drinks.Add(SelectedDrink);
                  }, obj => EditMode);//canExecute
            }
        }
        RelayCommand removeDrink;
        public RelayCommand RemoveDrink
        {
            get
            {
                return removeDrink ??=
                  new RelayCommand(obj =>
                  {
                      Drinks.Remove(SelectedDrink);
                      DrinksView = true;
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
                      Topings.Add(new("",0,0));
                  }, obj => EditMode);//canExecute
            }
        }
        RelayCommand removeToping;
        public RelayCommand RemoveToping
        {
            get
            {
                return removeToping ??=
                new RelayCommand(obj =>
                  {
                      Topings.Remove((obj as Topping));
                  });//canExecute
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
                    TopingView = false;
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
                  });//canExecute
            }
        }
        #endregion
    }
}
