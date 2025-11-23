using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace ClientCoopSoft.Converters
{
    public class BoolToBrushConverter : IValueConverter
    {
        private static readonly SolidColorBrush CerradaBrush =
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D9534F")); // rojo suave

        private static readonly SolidColorBrush AbiertaBrush =
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1D9942")); // verde suave

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool estado)
                return estado ? CerradaBrush : AbiertaBrush;

            return AbiertaBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == CerradaBrush;
        }
    }
}
