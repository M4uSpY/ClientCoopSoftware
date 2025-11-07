using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ClientCoopSoft.Converters
{
    public class BoolToGeneroConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is bool Genero)
            {
                return Genero ? "Masculino" : "Femenino";
            }
            return "Otro";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                return str.Equals("Masculino", StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }
    }
}
