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
        Wallet wal = new();
        public ModelBar ViewModelBar
        {
            get => viewModelBar;
            set => viewModelBar = value;
        }

        private string pathName;
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

        public int Wallet
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

        private string imageOrder;
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
            ViewModelBar.OnOrderCreate += ViewModelBar_OnOrderCreate;
            ViewModelBar.OrderCreatePercent = 40;
            InitializeComponent();
        }

        private async void ViewModelBar_OnOrderCreate()
        {
            ViewModelBar.Done = false;
            await Task.Delay(200);
            OnPropertyChanged(nameof(Wallet));
            AnimCup();
        }

        private async void AnimCup()
        {
            ImageOrder = ViewModelBar.SelectedDrink.ImagePath;
            DoubleAnimation fadeInAnimation = new DoubleAnimation();
            fadeInAnimation.From = 0;
            fadeInAnimation.To = 1;
            fadeInAnimation.Duration = new Duration(TimeSpan.FromSeconds(4));
            Cup.BeginAnimation(OpacityProperty, fadeInAnimation);
        }
        private void InitViewModelPath()
        {
            ViewModelBar.DrinkSrcPath =  @$"Resources\{PathName}Drinks.json";
            ViewModelBar.TopingSrcPath = @$"Resources\{PathName}Topings.json";
        }

        private void NewDirectory()
        {
            if (!Directory.Exists(Environment.CurrentDirectory + "/Resources"))
                Directory.CreateDirectory(Environment.CurrentDirectory + "/Resources");
            if (!File.Exists(Environment.CurrentDirectory + $"/Resources/{PathName}Drinks.json"))
                File.Create(Environment.CurrentDirectory + $"/Resources/{PathName}Drinks.json");
            if (!File.Exists(Environment.CurrentDirectory + $"/Resources/{PathName}Topings.json"))
                File.Create(Environment.CurrentDirectory + $"/Resources/{PathName}Topings.json");
        }

        private void AdminBtn_Click(object sender, RoutedEventArgs e)
        {
            ViewModelBar.EditMode = !ViewModelBar.EditMode;
        }

        private async void ChangeBTN_Click(object sender, RoutedEventArgs e)
        {
            if (Wallet > 0 && (ViewModelBar.OrderProgress >= 31 || ViewModelBar.OrderProgress == 0))
            {
                Wallet = 0;
                Coin.Visibility = Visibility.Visible;
                await Task.Delay(2000);
                Coin.Visibility = Visibility.Hidden;
            }
        }

        private void MoneyBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (wal != null)
                wal.Activate();
            else
                (wal ??= new()).Closed += WalletDestroy;
            wal.Show();
        }

        private void WalletDestroy(object? sender, EventArgs e)
        {
            wal = null;
        }

        private void Terminal_Drop(object sender, DragEventArgs e)
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

        private void Cup_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (ViewModelBar.OrderProgress == 100)
            {
                Cup.BeginAnimation(OpacityProperty, null);
                ViewModelBar.Done = true;
                Cup.Opacity = 0;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
