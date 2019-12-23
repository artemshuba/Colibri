using Jupiter.Application;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace Colibri.Helpers
{
    public static class ThemeHelper
    {
        public static void UpdateTitleBarColors()
        {
            var appView = ApplicationView.GetForCurrentView();
            Color bgColor = Colors.White;
            switch (JupiterApp.Current.NavigationService.Frame.RequestedTheme)
            {
                case ElementTheme.Default:
                    bgColor = Application.Current.RequestedTheme == ApplicationTheme.Light ? Colors.White : Colors.Black;
                    break;

                case ElementTheme.Light:
                    bgColor = Colors.White;
                    break;

                case ElementTheme.Dark:
                    bgColor = Colors.Black;
                    break;
            }
            appView.TitleBar.ButtonBackgroundColor = Color.FromArgb(0, bgColor.R, bgColor.G, bgColor.B);
            appView.TitleBar.ButtonInactiveBackgroundColor = appView.TitleBar.ButtonBackgroundColor;
        }
    }
}