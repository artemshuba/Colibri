using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Colibri.Services;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Colibri.Controls
{
    public sealed partial class AvatarControl : UserControl
    {
        public static readonly DependencyProperty AvatarProperty = DependencyProperty.Register(
            "Avatar", typeof(ImageSource), typeof(AvatarControl), new PropertyMetadata(default(ImageSource), AvatarPropertyChanged));

        private static void AvatarPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (AvatarControl)d;
            control.EllipseBrush.ImageSource = (ImageSource)e.NewValue;
//#if DEBUG
//            var bi = e.NewValue as BitmapImage;
//            if (bi != null)
//            {
//                if (bi.UriSource != null)
//                    Logger.Info("Loading avatar " + bi.UriSource.OriginalString);
//            }
//#endif
        }

        public ImageSource Avatar
        {
            get { return (ImageSource)GetValue(AvatarProperty); }
            set { SetValue(AvatarProperty, value); }
        }

        public AvatarControl()
        {
            this.InitializeComponent();
        }

        private void EllipseBrush_OnImageOpened(object sender, RoutedEventArgs e)
        {
//#if DEBUG
//            var image = (ImageBrush)sender;
//            var bi = image.ImageSource as BitmapImage;
//            if (bi != null)
//            {
//                if (bi.UriSource != null)
//                    Logger.Info("Loaded avatar " + bi.UriSource.OriginalString);
//            }
//#endif
        }

        private void EllipseBrush_OnImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
//#if DEBUG
//            var image = (ImageBrush)sender;
//            var bi = image.ImageSource as BitmapImage;
//            if (bi != null)
//            {
//                if (bi.UriSource != null)
//                    Logger.Info("Failed loading avatar " + bi.UriSource.OriginalString);
//            }
//#endif
        }
    }
}