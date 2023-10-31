using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CoffeMachineNew.Win
{
    /// <summary>
    /// Логика взаимодействия для Wallet.xaml
    /// </summary>
    public partial class Wallet : Window
    {
        public Wallet()
        {
            InitializeComponent();
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Image money = (Image)sender;
            DragDrop.DoDragDrop(money, money.Tag.ToString(), DragDropEffects.Move);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;//пропускаем закрытие
            Visibility = Visibility.Hidden;//скрываем окно
        }
    }
}
