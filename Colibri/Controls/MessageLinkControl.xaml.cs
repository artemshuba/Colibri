using System;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VkLib.Core.Attachments;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Colibri.Controls
{
    public sealed partial class MessageLinkControl : UserControl
    {
        public VkLinkAttachment Link { get; set; }

        public MessageLinkControl(VkLinkAttachment link)
        {
            Link = link;

            this.InitializeComponent();

            try
            {
                var uri = new Uri(link.Url);
                HostTextBlock.Text = uri.Host;
            }
            catch { }
        }

        private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri(Link.Url));
        }
    }
}