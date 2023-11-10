using CoffeMachineNew.ViewModel;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.XPath;
using static System.Net.Mime.MediaTypeNames;

namespace CoffeMachineNew.Win
{
    /// <summary>
    /// Логика взаимодействия для CoffMach.xaml
    /// </summary>
    public partial class CoffMach : UserControl, INotifyPropertyChanged
    {

        ModelBar viewModelBar = new();
        Wallet wal = new();// кошель
        public ModelBar ViewModelBar
        {
            get => viewModelBar;
            set => viewModelBar = value;
        }

        private string pathName;//имя для названия файлов
        public string PathName
        {
            get => pathName;
            set
            {
                if (pathName != value)
                {
                    pathName = value;
                    NewDirectory();
                    InitViewModelPath();
                    OnPropertyChanged(PathName);
                }
            }
        }

        public int Wallet// сколько заплатил пользователь
        {
            get => viewModelBar.Wallet;
            set
            {
                if (viewModelBar.Wallet != value)
                {
                    viewModelBar.Wallet = value;
                    OnPropertyChanged(nameof(Wallet));
                }
            }
        }

        private string imageOrder;// картинка для анимации готового продукта 
        public string ImageOrder
        {
            get => imageOrder;
            set
            {
                if (imageOrder != value)
                {
                    imageOrder = value;
                    OnPropertyChanged(nameof(ImageOrder));
                }
            }
        }

        public CoffMach()
        {
            DataContext = this;
            ViewModelBar.OnOrderCreate += ViewModelBar_OnOrderCreate;//отдаем делегату что делать при заказе 
            ViewModelBar.OrderCreatePercent = 30;// на каком проценте будет начало приготовления
            InitializeComponent();
        }

        private async void ViewModelBar_OnOrderCreate()
        {
            ViewModelBar.Done = false;//флаг что еще не приготовлено
            await Task.Delay(200);//немного ждем
            OnPropertyChanged(nameof(Wallet));//обновления для отображения сколько денег доступно
            AnimCup();
        }

        private async void AnimCup()// анимация появления чашки
        {
            ImageOrder = ViewModelBar.SelectedDrink.ImagePath;//Путь к картинке из выбранного напитка
            DoubleAnimation fadeInAnimation = new DoubleAnimation();//объявляем анимацию
            //диапохон значений
            fadeInAnimation.From = 0;
            fadeInAnimation.To = 1;
            fadeInAnimation.Duration = new Duration(TimeSpan.FromSeconds(6));//время анимации
            Cup.BeginAnimation(OpacityProperty, fadeInAnimation);//запуск анимации
        }
        private void InitViewModelPath()
        {
            ViewModelBar.DrinkSrcPath =  @$"Resources\{PathName}Drinks.json";//пути к файлам
            ViewModelBar.TopingSrcPath = @$"Resources\{PathName}Topings.json";
        }

        private void NewDirectory()//создание нужных файлов
        {
            if (!Directory.Exists(Environment.CurrentDirectory + "/Resources"))
                Directory.CreateDirectory(Environment.CurrentDirectory + "/Resources");
            if (!File.Exists(Environment.CurrentDirectory + $"/Resources/{PathName}Drinks.json"))
                using (File.Create(Environment.CurrentDirectory + $"/Resources/{PathName}Drinks.json")) { }
            if (!File.Exists(Environment.CurrentDirectory + $"/Resources/{PathName}Topings.json"))
                using (File.Create(Environment.CurrentDirectory + $"/Resources/{PathName}Topings.json")) { }
        }

        private void AdminBtn_Click(object sender, RoutedEventArgs e)//админ панель
        {
            ViewModelBar.EditMode = !ViewModelBar.EditMode;
        }

        private async void ChangeBTN_Click(object sender, RoutedEventArgs e)//выдать сдачу
        {
            if (Wallet > 0 && (ViewModelBar.OrderProgress >= 31 || ViewModelBar.OrderProgress == 0))
            {
                Wallet = 0;
                Coin.Visibility = Visibility.Visible;
                await Task.Delay(2000);
                Coin.Visibility = Visibility.Hidden;
            }
        }

        private void MoneyBar_MouseDown(object sender, MouseButtonEventArgs e)//появление кошелька
        {
            wal.Show();
        }

        private void Terminal_Drop(object sender, DragEventArgs e)//реализация дагдропа
        {
            try
            {
                string tstring = e.Data.GetData(typeof(string)).ToString();
                Wallet += Int32.Parse(tstring);
                OnPropertyChanged(nameof(Wallet));
            } catch {
                return;
            }
        }

        private void Cup_MouseDown(object sender, MouseButtonEventArgs e)//забрать напиток
        {
            if (ViewModelBar.OrderProgress == 100)
            {
                Cup.BeginAnimation(OpacityProperty, null);
                ViewModelBar.Done = true;
                Cup.Opacity = 0;
                if (ViewModelBar.DrinksView == false)
                {
                    ViewModelBar.ProgressView = false;
                    ViewModelBar.DrinksView = true;
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
