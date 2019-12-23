using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace Colibri.Converters
{
    public class ListItemClickArgsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var args = (ItemClickEventArgs)value;
            var element = args.ClickedItem;

            return element;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
