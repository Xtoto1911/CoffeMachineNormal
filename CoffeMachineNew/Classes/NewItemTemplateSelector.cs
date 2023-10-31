using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows;

namespace CoffeMachineNew.Classes
{
    public sealed class NewItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ItemTemplate { get; set; } // Объявление свойства,
                                                       // которое будет использоваться для отображения элементов коллекции

        public DataTemplate NewItemPlaceholderTemplate { get; set; }//использоваться для отображения плейсхолдера нового элемента в коллекции
        /// <summary>
        /// Переопределение метода SelectTemplate, который выбирает шаблон для отображения элемента коллекции
        /// </summary>
        /// <param name="item"></param>
        /// <param name="container"></param>
        /// <returns></returns>
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            //Проверка, является ли элемент коллекции плейсхолдером нового элемента
            if (item == CollectionView.NewItemPlaceholder)
            {
                // Если да, то возвращается шаблон для отображения плейсхолдера нового элемента
                return NewItemPlaceholderTemplate;
            }
            // Если элемент не является плейсхолдером нового элемента, то возвращается шаблон для отображения элемента коллекции
            return ItemTemplate;
        }
    }
}
