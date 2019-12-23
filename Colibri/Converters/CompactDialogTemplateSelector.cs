using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Colibri.Model;

namespace Colibri.Converters
{
    public class CompactDialogTemplateSelector : DataTemplateSelector
    {
        public DataTemplate CompactDialogTemplate { get; set; }

        public DataTemplate CompactGroupChatTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var dialog = (Dialog)item;
            if (dialog == null)
                return null;

            if (dialog.Message.ChatId != 0)
                return CompactGroupChatTemplate;

            return CompactDialogTemplate;
        }
    }
}