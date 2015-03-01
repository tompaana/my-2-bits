using System;
using System.Collections.Generic;
using System.Text;

#if NETFX_CORE
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
#elif SILVERLIGHT
using System.Windows;
using System.Windows.Data;
#endif

namespace SlidingPanelSample
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
#if NETFX_CORE
        public object Convert(object value, Type targetType, object parameter, string language)
#elif SILVERLIGHT
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo cultureInfo)
#endif
        {
            if (!(value is bool))
            {
                throw new ArgumentException("The given value has to be of type boolean");
            }

            bool boolValue = (bool)value;

            if (parameter is bool && (bool)parameter == true)
            {
                boolValue = !boolValue;
            }

            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

#if NETFX_CORE
        public object ConvertBack(object value, Type targetType, object parameter, string language)
#elif SILVERLIGHT
        public object ConvertBack(object value, Type targetType, object paramenter, System.Globalization.CultureInfo cultureInfo)
#endif
        {
            throw new NotImplementedException();
        }
    }
}
