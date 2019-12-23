using System;
using Windows.UI.Xaml.Data;
using Colibri.Helpers;

namespace Colibri.Converters
{
    public class MessageTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            var date = (DateTime)value;
            var hours = (DateTime.Now.ToUniversalTime() - date).TotalHours;

            if (date.Date == DateTime.Today)
                return date.ToString("t");

            if (DateTime.Today - date.Date == TimeSpan.FromDays(1))
                return Localizator.String("Yesterday").ToLower();

            return date.ToString(Localizator.String("MessageTimeFormat"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }
}
