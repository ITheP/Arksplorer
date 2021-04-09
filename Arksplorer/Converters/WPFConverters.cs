using System;
using System.Globalization;
using System.Windows.Data;

namespace Arksplorer
{
    public class ArkColorDBNullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == DBNull.Value)
            return null;

            return ((ArkColor)value).Color;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
