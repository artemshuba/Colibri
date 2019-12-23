using Windows.UI.Xaml;
using Jupiter.Services.Settings;
using VkLib.Core.Auth;

namespace Colibri
{
    public static class AppSettings
    {
        public static VkAccessToken AccessToken
        {
            get { return SettingsService.Roaming.Get<VkAccessToken>(); }
            set { SettingsService.Roaming.Set(value); }
        }

        public static ApplicationTheme? AppTheme
        {
            get { return SettingsService.Roaming.Get<ApplicationTheme?>(); }
            set { SettingsService.Roaming.Set(value); }
        }

        public static bool EnableNotifications
        {
            get { return SettingsService.Local.Get(defaultValue: true); }
            set { SettingsService.Local.Set(value); }
        }

        public static int NewDialogFriendsSortTypeIndex
        {
            get { return SettingsService.Roaming.Get(defaultValue: 1); }
            set { SettingsService.Roaming.Set(value); }
        }

        //Invisible mode settings

        public static bool HideOnlineStatus
        {
            get { return SettingsService.Local.Get(defaultValue: false); }
            set { SettingsService.Local.Set(value); }
        }

        public static bool DontMarkMessagesAsRead
        {
            get { return SettingsService.Local.Get(defaultValue: false); }
            set { SettingsService.Local.Set(value); }
        }

        public static bool EnableExtendedNotifications
        {
            get { return SettingsService.Local.Get(defaultValue: true); }
            set { SettingsService.Local.Set(value); }
        }
    }
}