using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Colibri.Model;

namespace Colibri.Converters
{
    public class DialogBodyContentTemplateSelector : DataTemplateSelector
    {
        public DataTemplate InboxTemplate { get; set; }

        public DataTemplate OutboxTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var dialog = (Dialog)item;
            if (dialog == null)
                return null;

            return dialog.Message.IsOut ? OutboxTemplate : InboxTemplate;
        }
    }
}
