using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Colibri.Helpers;
using Colibri.View;
using VkLib.Core.Attachments;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Colibri.Controls
{
    public sealed partial class MessageVideoControl : UserControl
    {
        public VkVideoAttachment Video { get; set; }

        public MessageVideoControl(VkVideoAttachment video)
        {
            Video = video;

            this.InitializeComponent();
        }

        private void UIElement_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            var p = new Dictionary<string, object>();
            p.Add("video", Video);
            Navigator.NavigateAdaptive(typeof(VideoPreviewView), p);
        }

        private void Photo_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            TitleTextBlock.MaxWidth = Math.Max(20, e.NewSize.Width - 20 - DurationTextBlock.ActualWidth - 10);
        }
    }
}