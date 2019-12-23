using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Colibri.Model;

namespace Colibri.Converters
{
    public class ChatAttachmentUploadTemplateSelector : DataTemplateSelector
    {
        public DataTemplate PhotoTemplate { get; set; }

        public DataTemplate DocumentTemplate { get; set; }

        public DataTemplate AudioTemplate { get; set; }

        public DataTemplate VideoTemplate { get; set; }

        public DataTemplate ForwardsTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var attachment = item as AttachmentUpload;
            if (attachment != null)
            {
                if (attachment is PhotoAttachmentUpload)
                    return PhotoTemplate;

                if (attachment is DocumentAttachmentUpload)
                    return DocumentTemplate;

                if (attachment is VideoAttachmentUpload)
                    return VideoTemplate;

                if (attachment is ForwardMessagesAttachmentUpload)
                    return ForwardsTemplate;
            }

            return null;
        }
    }
}
