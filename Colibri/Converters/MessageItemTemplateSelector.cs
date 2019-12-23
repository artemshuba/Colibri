using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Colibri.Model;

namespace Colibri.Converters
{
    public class MessageItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate InboxTemplate { get; set; }

        public DataTemplate OutboxTemplate { get; set; }

        public DataTemplate ChatInboxTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var message = (Message)item;
            if (message.MessageContent.IsOut)
                return OutboxTemplate;
            else if (message.MessageContent.ChatId != 0)
                return ChatInboxTemplate;
            else
                return InboxTemplate;
        }
    }
}