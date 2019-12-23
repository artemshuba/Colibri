using System;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Colibri.Helpers;
using VkLib.Core.Attachments;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Colibri.Controls
{
    public sealed partial class MessageWallPostControl : UserControl
    {
        public VkWallPostAttachment WallPost { get; set; }

        public MessageWallPostControl(VkWallPostAttachment wallPost)
        {
            WallPost = wallPost;

            this.InitializeComponent();

            if (string.IsNullOrEmpty(wallPost.Text))
            {
                TitleTextBlock.Text = Localizator.String("ChatMessageWallPostTitle");
                DateTextBlock.Text = wallPost.Date.ToString(Localizator.String("MessageWallPostTimeFormat"));
            }
            else
            {
                TitleTextBlock.Text = WallPost.Text;       
                DateTextBlock.Text = Localizator.String("ChatMessageWallPostTitle") + ", " + wallPost.Date.ToString(Localizator.String("MessageWallPostTimeFormat"));
            }
        }

        private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri(WallPost.GetLink()));
        }
    }
}