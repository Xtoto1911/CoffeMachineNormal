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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CoffeMachineNew.Win
{
    /// <summary>
    /// Логика взаимодействия для CoffBar.xaml
    /// </summary>
    public partial class CoffBar : UserControl
    {
        public CoffBar()
        {
            InitializeComponent();
        }

        private void ScrollLeft_Click(object sender, RoutedEventArgs e)
        {
            var sv = GetScrollViewer(DrinksList);
            if (sv != null)
            {
                sv.ScrollToHorizontalOffset(sv.HorizontalOffset - 310); // Измените значение для скорости прокрутки
            }
        }

        private void ScrollRight_Click(object sender, RoutedEventArgs e)
        {
            var sv = GetScrollViewer(DrinksList);
            if (sv != null)
            {
                sv.ScrollToHorizontalOffset(sv.HorizontalOffset + 310); // Измените значение для скорости прокрутки
            }
        }

        private ScrollViewer GetScrollViewer(DependencyObject depObj)//поиск рекурсивно бежим в поисках скрола 
        {
            if (depObj is ScrollViewer scrollViewer)
            {
                return scrollViewer;
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);
                var result = GetScrollViewer(child);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }
    }
}
