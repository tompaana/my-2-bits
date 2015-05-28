using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace BLEBeaconSample
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool valueAsBool = (value is bool && (bool)value);
            Visibility visibility = valueAsBool ? Visibility.Visible : Visibility.Collapsed;

            if (parameter != null && parameter is string && (parameter as string).Equals("Inverse"))
            {
                visibility = valueAsBool ? Visibility.Collapsed : Visibility.Visible;
            }

            return visibility;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class InvertValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool)
            {
                return !((bool)value);
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToAdvertisingButtonTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool valueAsBool = (value is bool && (bool)value);
            return valueAsBool ? "stop advertising" : "start advertising";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
