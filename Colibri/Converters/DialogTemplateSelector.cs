using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Colibri.Model;

namespace Colibri.Converters
{
    public class DialogTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DialogTemplate { get; set; }

        public DataTemplate GroupChatTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var dialog = (Dialog)item;
            if (dialog == null)
                return null;

            if (dialog.Message.ChatId != 0)
                return GroupChatTemplate;

            return DialogTemplate;
        }
    }
}