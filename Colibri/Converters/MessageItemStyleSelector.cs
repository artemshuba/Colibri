using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Colibri.Model;

namespace Colibri.Converters
{
    public class MessageItemStyleSelector : StyleSelector
    {
        public Style MessageStyle { get; set; }

        public Style ServiceMessageStyle { get; set; }

        protected override Style SelectStyleCore(object item, DependencyObject container)
        {
            var message = (Message)item;
            if (!string.IsNullOrEmpty(message.MessageContent.Action))
                return ServiceMessageStyle;

            return MessageStyle;
        }
    }
}
