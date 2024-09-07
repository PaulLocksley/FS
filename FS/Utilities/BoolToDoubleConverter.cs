using System.Globalization;

namespace FS.Utilities;

public class BoolToFloatConverter : IValueConverter
{
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? 1.0 : 0.0;
            }
            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is float doubleValue)
            {
                return doubleValue == 1.0;
            }
            return false;
        }
}