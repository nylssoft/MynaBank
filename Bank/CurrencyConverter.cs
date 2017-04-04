using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Bank
{
    public class CurrencyConverter : IValueConverter
    {
        public static CultureInfo Culture { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is long)
            {
                return ConvertToCurrencyString((long)value);
            }
            return "<undefined>";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public static long ParseCurrency(string txt)
        {
            if (decimal.TryParse(txt, out decimal ret))
            {
                return (long)(ret * 100m);
            }
            return (long)(decimal.Parse(txt, System.Globalization.NumberStyles.Any) * 100m);
        }

        public static string ConvertToInputString(long val)
        {
            decimal v = val / 100m;
            return System.Convert.ToString(v);
        }

        public static string ConvertToCurrencyString(long val)
        {
            decimal v = val / 100m;
            return v.ToString("c", Culture);
        }
    }
}
