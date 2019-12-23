using System;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VkLib.Core.Attachments;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Colibri.Controls
{
    public sealed partial class MessageDocumentControl : UserControl
    {
        public VkDocumentAttachment Document { get; set; }

        public MessageDocumentControl(VkDocumentAttachment document)
        {
            this.Document = document;

            this.InitializeComponent();

            if (!string.IsNullOrEmpty(document.Photo130))
                ContentControl.ContentTemplate = (DataTemplate)Resources["ImageDocumentTemplate"];
            else
                ContentControl.ContentTemplate = (DataTemplate)Resources["GenericDocumentTemplate"];
        }

        private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri(Document.Url));
        }
    }
}
