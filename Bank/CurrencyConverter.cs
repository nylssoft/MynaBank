/*
    Myna Bank
    Copyright (C) 2017 Niels Stockfleth

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;
using System.Globalization;
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
