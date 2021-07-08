using System;
using System.Globalization;
using Xamarin.Forms;

namespace OWCE.Converters
{
    // Converts from volts (float) to battery percentage (string)
    // Eg 62.3 -> "98%"
    public class QuartVoltageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is float batteryVoltage)
            {
                double pct = 99.9 / (0.8 + Math.Pow(1.28, 54 - batteryVoltage)) - 9;
                int pctInt = (int)pct;
                return String.Format("{0}%", pctInt);
            }
            return "0%";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
