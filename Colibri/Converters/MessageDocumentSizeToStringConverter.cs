using System;
using Windows.UI.Xaml.Data;

namespace Colibri.Converters
{
    public class MessageDocumentSizeToStringConverter : IValueConverter
    {
        private const long OneKiloByte = 1024;
        private const long OneMegaByte = OneKiloByte * 1024;
        private const long OneGigaByte = OneMegaByte * 1024;


        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var size = System.Convert.ToDouble(value);

            string suffix;
            if (size > OneGigaByte)
            {
                size /= OneGigaByte;
                suffix = " GB";
            }
            else if (size > OneMegaByte)
            {
                size /= OneMegaByte;
                suffix = " MB";
            }
            else if (size > OneKiloByte)
            {
                size /= OneKiloByte;
                suffix = " KB";
            }
            else
            {
                suffix = " B";
            }

            return String.Format("{0:N2}{1}", size, suffix);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
