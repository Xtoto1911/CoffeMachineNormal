using CoffeMachineNew.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CoffeMachineNew.Win
{
    /// <summary>
    /// Логика взаимодействия для CoffMach.xaml
    /// </summary>
    public partial class CoffMach : UserControl, INotifyPropertyChanged
    {

        ModelBar viewModelBar = new ModelBar();
        public ModelBar ViewModelBar
        {
            get => viewModelBar;
            set => viewModelBar = value;
        }

        public CoffMach()
        {
            DataContext = this;
            NewDirectory();
            InitViewModelPath();
            InitializeComponent();
        }

        private void InitViewModelPath()
        {
            ViewModelBar.DrinkSrcPath =  @$"Resources\{this.Name}Drinks.json";
            ViewModelBar.TopingSrcPath = @$"Resources\{this.Name}Topings.json";
        }

        private void NewDirectory()
        {
            if (!Directory.Exists(Environment.CurrentDirectory + "/Resources"))
                Directory.CreateDirectory(Environment.CurrentDirectory + "/Resources");
            if (!File.Exists(Environment.CurrentDirectory + $"/Resources/{this.Name}Drinks.json"))
                File.Create(Environment.CurrentDirectory + $"/Resources/{this.Name}Drinks.json");
            if (!File.Exists(Environment.CurrentDirectory + $"/Resources/{this.Name}Topings.json"))
                File.Create(Environment.CurrentDirectory + $"/Resources/{this.Name}Topings.json");
        }

        private void AdminBtn_Click(object sender, RoutedEventArgs e)
        {
            ViewModelBar.EditMode = !ViewModelBar.EditMode;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
