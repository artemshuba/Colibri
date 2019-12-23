using System;
using Windows.System.Profile;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Colibri.Controls;
using Jupiter.Services.Navigation;
using Windows.UI.Xaml;
using Jupiter.Application;

namespace Colibri.Helpers
{
    /// <summary>
    /// Simple helper class to simplify navigation with multiple frames
    /// </summary>
    public class Navigator
    {
        public static NavigationService Main => JupiterApp.Current.NavigationService;

        public static NavigationService Content
        {
            get
            {
                //on Desktop there will be Content navigation service
                if (JupiterApp.Current.NavigationServices.IsRegistered("Content"))
                    return JupiterApp.Current.NavigationServices["Content"];

                //otherwise return default
                return JupiterApp.Current.NavigationService;
            }
        }

        public static void NavigateAdaptive(Type page, object parameter = null, bool clearHistory = false, NavigationTransitionInfo infoOverride = null)
        {
            if (AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
            {
                Main.Navigate(page, parameter, clearHistory, infoOverride);
            }
            else
            {
                var flyout = new PopupControl();
                var frame = new Frame();
                frame.Navigate(page, parameter, new SuppressNavigationTransitionInfo());
                flyout.FlyoutContent = frame;
                flyout.Show();
            }
        }

        public static void ClearPageCache()
        {
            var cacheSize = Main.Frame.CacheSize;
            Main.Frame.CacheSize = 0;
            Main.Frame.CacheSize = cacheSize;
        }
    }
}