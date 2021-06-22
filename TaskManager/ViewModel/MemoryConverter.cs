using System;
using System.Globalization;
using System.Windows.Data;

namespace TaskManager.ViewModel
{
    //Конвертёр памяти 
    public class MemoryConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //Используемая память возвращается в виде байтов, она приводится к килобайтам.
            return (value as long?) / 1024;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
