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
            /// <summary>
            /// Записать лист в Json
            /// </summary>
            /// <param name="items">Коллекция</param>
            /// <param name="pathJSON">Путь до файла</param>
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

            /// <summary>
            /// Считать лист из Json
            /// </summary>
            /// <param name="src">путь к файлу</param>
            /// <returns></returns>
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

            /// <summary>
            /// Записать айтем в коллекцию
            /// </summary>
            /// <param name="item"></param>
            /// <param name="items"></param>
            public static void AddItemToJSON(T item, Collection<T> items)
            {
                if (!items.Contains(item))
                    items.Add(item);
            }

             /// <summary>
             /// Вытащить из коллекции
             /// </summary>
             /// <param name="item"></param>
             /// <param name="items"></param>
             /// <returns></returns>
            public static T GetItem(T item, Collection<T> items)
            {
                if (items.Contains(item))
                    return items[items.IndexOf(item)];
                return default;
            }

            /// <summary>
            /// Вытащить из коллекции по айди
            /// </summary>
            /// <param name="idItem"></param>
            /// <param name="items"></param>
            /// <returns></returns>
            public static T GetItem(int idItem, Collection<T> items)
            {
                foreach (T item in items)
                    if ((item as Product).ID == idItem) return item;
                return default;
            }
        }
        #endregion
        #region Комманды
        private RelayCommand changeAvatar;//команда смены картинки 
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

        private void AvaterPathSelect()
        {
            OpenFileDialog file = new OpenFileDialog();
            file.Filter = "Изображения (*.jpg;*.png)|*.jpg;*.png";
            file.ShowDialog();
            if (file.FileName != "" && file.FileName != SelectedDrink.ImagePath)
                SelectedDrink.ImagePath = file.FileName;
        }

        private RelayCommand showDrinks;
        public RelayCommand ShowDrinks// конда для выхода к листу с напитками
        {
            get
            {
                return showDrinks ??=
                  new RelayCommand(obj =>
                  {//что сделает команда
                      DrinksView = true;
                      ProgressView = false;
                      SelectedDrink = null;
                  }, obj => SelectedDrink != null && SelectedDrink.Name != "");//условие при которых она может быть выполнена
            }
        }

        private RelayCommand showTopingsToggle;
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

        private RelayCommand upCount;//пользователь увеличивает количество нужного доппинга
        public RelayCommand UpCount
        {
            get
            {
                return upCount ??=
                  new RelayCommand(obj =>
                  {
                      (obj as ToppingNode).Count++;
                      Sum += (obj as ToppingNode).Topping.Price;
                  }, obj => obj != null && (obj as ToppingNode).Count < (obj as ToppingNode).Topping.MaxCnt);
            }
        }

        private RelayCommand downCount;//пользователь уменьшает количество доппинго
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

        private RelayCommand addNode;// добавление пользователем доппинга в выбранные
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

        private async void ProgressTic(int prcnt)//имитация приготовления
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
        }

        private RelayCommand createOrder;
        public RelayCommand CreateOrder//Комманда для заказа
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

        private RelayCommand denyOrder;
        public RelayCommand DenyOrder//Отменить заказ или если уже приготовлено то выход к листу с напитками
        {
            get
            {
                return OrderProgress == 100 ? ShowDrinks : denyOrder ??=
                  new RelayCommand(obj =>
                  {
                      OrderProgress = 100;
                      ProgressView = false;
                  }, obj => OrderProgress < OrderCreatePercent || OrderProgress == 100);
            }
        }

        private RelayCommand addDrink;//Кнопка добавления нового напитка
        public RelayCommand AddDrink
        {
            get
            {
                return addDrink ??=
                  new RelayCommand(obj =>
                  {
                      SelectedDrink = new("", 0, "");
                      Drinks.Add(SelectedDrink);
                  }, obj => EditMode);
            }
        }

        private RelayCommand removeDrink;//кнопка удаления напитка
        public RelayCommand RemoveDrink
        {
            get
            {
                return !Removing && !IsChecked ? ConfirmRemove : removeDrink ??=
                  new RelayCommand(obj =>
                  {
                      Drinks.Remove(SelectedDrink);
                      DrinksView = true;
                      Removing = false;
                  }, obj => true);//canExecute
            }
        }

        private RelayCommand addToping;// кнопка добавления топпинга в лист со всеми топпингами 
        public RelayCommand AddToping
        {
            get
            {
                return addToping ??=
                  new RelayCommand(obj =>
                  {
                      Topings.Add(new("", 0, 0));
                  }, obj => EditMode);
            }
        }

        private RelayCommand removeToping;//удаление топпинга из листа всех топпинго
        public RelayCommand RemoveToping
        {
            get
            {
                return !Removing && !IsChecked ? ConfirmRemove : removeToping ??=
                  new RelayCommand(obj =>
                  {
                      Topings.Remove(obj as Topping);
                      Removing = false;
                      foreach (Drink drink in Drinks)
                          drink.Toppings.Remove((obj as Topping).ID);
                  });
            }
        }

        private RelayCommand addDrinkToping;//добавление топпинга в напиток
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
                  }, obj => obj != null && (obj as Topping).Name != "" && !DrinkTopings.Contains(obj as Topping));
            }
        }

        private RelayCommand removeDrinkToping;//удаление топпинга из напитков
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

        private RelayCommand confirmRemove;
        public RelayCommand ConfirmRemove//команда для отображения предупреждения
        {
            get
            {
                return confirmRemove ??=
                  new RelayCommand(obj =>
                  {
                      ConfirmView = true;
                  }, obj => true);
            }
        }

        private RelayCommand removeAnsw;//для того убрать панель предупреждения
        public RelayCommand RemoveAnsw
        {
            get
            {
                return removeAnsw ??=
                  new RelayCommand(obj =>
                  {
                      Removing = bool.Parse((string)obj);
                      ConfirmView = false;
                  }, obj => true);
            }
        }

        #endregion
        #region Хранение
        ObservableCollection<Drink> drinks;// коллекция напитков
        public ObservableCollection<Drink> Drinks
        {
            set => drinks = value;
            get
            {
                drinks ??= new();//если коллекция пуста создаем новую
                var NewItem = (ListCollectionView)CollectionViewSource.GetDefaultView(drinks);
                NewItem.NewItemPlaceholderPosition = EditMode ? NewItemPlaceholderPosition.AtEnd : NewItemPlaceholderPosition.None;//для добавления нового напитка,
                                                                                                                                   //добавляем образ нового объекта
                return drinks;
            }
        }

        ObservableCollection<Topping> topings;
        public ObservableCollection<Topping> Topings
        {
            set => topings = value;
            get
            {
                topings ??= new();
                var NewItem = (ListCollectionView)CollectionViewSource.GetDefaultView(topings);
                NewItem.NewItemPlaceholderPosition = EditMode ? NewItemPlaceholderPosition.AtEnd : NewItemPlaceholderPosition.None;
                return topings;
            }
        }

        string drinkScrPath = string.Empty;//путь к джейсонфайлу напитка
        public string DrinkSrcPath
        {
            get => drinkScrPath;
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
            get => topingScrPath;
            set
            {
                if (topingScrPath != value)
                {
                    topingScrPath = value;
                    Topings = ReductionItems<Topping>.GetItemlist(TopingSrcPath);
                    var topingView = (ListCollectionView)CollectionViewSource.GetDefaultView(Topings);
                    topingView.NewItemPlaceholderPosition = EditMode ? NewItemPlaceholderPosition.AtEnd : NewItemPlaceholderPosition.None;
                    OnPropertyChanged(nameof(Topings));
                }
            }
        }

        int wallet;//сколько денег вложил пользователь
        public int Wallet
        {
            get { return wallet; }
            set
            {
                if (wallet != value)
                {
                    wallet = value;
                    OnPropertyChanged(nameof(Wallet));
                }
            }
        }

        public bool Done = true;// flag Готовность заказа

        #endregion
        #region Настройки отображения
        private bool editMode;
        public bool EditMode //флаг для админ режима
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
        
        private bool confirmView;
        public bool ConfirmView// флаг для отображения оповещения об удалении
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

        private bool drinksView = true;
        public bool DrinksView// флаг для панели отображения листа с напитками
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

        private bool topingsView;
        public bool TopingsView// флаг для отображения листа с доппингами
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

        private bool progressView;
        public bool ProgressView// для отображения панели с прогрессбаром
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

        public bool HasToppings//добавить ли выбранный пользователем топпинг
        {
            get => DrinkTopings.Count > 0;
        }

        #endregion
        #region Для панели заказа

        private ObservableCollection<Topping> drinkTopings;//Доступные пользователю доппинги
        public ObservableCollection<Topping> DrinkTopings
        {
            get => drinkTopings ??= new();
            set => drinkTopings = value;
        }

        private ObservableCollection<ToppingNode> selectedTopings;//Выбранные пользователем доппинги
        public ObservableCollection<ToppingNode> SelectedTopings
        {
            get => selectedTopings ??= new();
            set => selectedTopings = value;
        }

        private void GetDrinkTopings()
        {
            DrinkTopings = new();//Сохдаем новый лист доступные для пользователя топпинги
            foreach (int id in SelectedDrink.Toppings)//записываем в доступные из выбранных по айди
                ReductionItems<Topping>.AddItemToJSON(ReductionItems<Topping>.GetItem(id, Topings), DrinkTopings);
            //если активирован режим админа то добавляет в конец кнопки для добавления
            var topingView = (ListCollectionView)CollectionViewSource.GetDefaultView(DrinkTopings);
            topingView.NewItemPlaceholderPosition = EditMode ? NewItemPlaceholderPosition.AtBeginning : NewItemPlaceholderPosition.None;
            OnPropertyChanged(nameof(DrinkTopings));
            OnPropertyChanged(nameof(HasToppings));
        }

        private void DrinkSelecting()
        {
            DrinksView = false;
            Sum = SelectedDrink.Price;
            GetDrinkTopings();
        }

        private Drink selectedDrink;//Выбранный напиток
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
        }

        int sum; 
        public int Sum//сумма заказа
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
        public int OrderProgress//процент прогресса
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
                    return $"Идет подготовка... {OrderProgress}%\n(До начала приготовления есть возможность отмены)";
                if (OrderProgress == 100)
                    return "Осторожно, горячо!!!\n Заберите напиток и сдачу";
                return $"Готовим.. {OrderProgress}%";
            }
        }
        public delegate void OrderCreate();//делигат для метода что будет при начале приготовления
        public event OrderCreate? OnOrderCreate;
        public int OrderCreatePercent { get; set; }//когда каком проценте начинается приготовление

        #endregion
        #region Редактирование

        private bool removing;// влаг для возможность удаления
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

        

        private bool isChecked;//для запоминания чек докса чтобы не повторять вывод предупреждения
        public bool IsChecked
        {
            get { return isChecked; }
            set
            {
                if (isChecked != value)
                {
                    isChecked = value;
                    OnPropertyChanged(nameof(IsChecked));
                }
            }
        }
        #endregion
    }
}
