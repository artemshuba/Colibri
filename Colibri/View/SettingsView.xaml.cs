using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Colibri.Helpers;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Colibri.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsView : Page
    {
        public string Version
        {
            get { return AppHelper.GetAppVersionString(); }
        }

        public SettingsView()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                var statusBar = StatusBar.GetForCurrentView();
                statusBar.BackgroundColor = ((SolidColorBrush)LayoutRoot.Background).Color;
                statusBar.BackgroundOpacity = 1;
            }

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                var statusBar = StatusBar.GetForCurrentView();
                statusBar.BackgroundColor = null;
                statusBar.BackgroundOpacity = 0;
            }

            base.OnNavigatingFrom(e);
        }

        private void ExtendedNotificationsPrivacyReadMoreHyperlink_OnClick(Hyperlink sender, HyperlinkClickEventArgs args)
        {
            var flyout = FlyoutBase.GetAttachedFlyout(ExtendedNotificationsPrivacyTextBlock);
            flyout.ShowAt(ExtendedNotificationsPrivacyTextBlock);
        }
    }
}