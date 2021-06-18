using System;
using System.Globalization;
using System.Windows.Data;

namespace TaskManager.ViewModel
{
    public class RefreshTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //value - переменная в TaskViewModel(refreshTime). Parameter - параметр, передаваемый от привязанного элемента.
            int temp = (int)value;

            //При первой привязке производится установка значений всех привязанных элементов. 
            //Если значение переменной совпадает с параметром привязанного элемента, то возвращается true (для IsChecked).
            if (temp == int.Parse(parameter.ToString()))
                return true;

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return parameter;
        }
    }
}
