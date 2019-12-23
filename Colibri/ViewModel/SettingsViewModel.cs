using System.Collections.Generic;
using System.Linq;
using Windows.Globalization;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Colibri.Helpers;
using Colibri.Model;
using GalaSoft.MvvmLight.Command;
using Jupiter.Mvvm;
using Jupiter.Application;

namespace Colibri.ViewModel
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly List<SettingsAppTheme> _appThemes = new List<SettingsAppTheme>()
        {
            new SettingsAppTheme(Localizator.String("SettingsAppThemeDefault"), ElementTheme.Default),
            new SettingsAppTheme(Localizator.String("SettingsAppThemeLight"), ElementTheme.Light ),
            new SettingsAppTheme(Localizator.String("SettingsAppThemeDark"), ElementTheme.Dark )
        };

        private readonly List<SettingsAppLanguage> _appLanguages = new List<SettingsAppLanguage>()
        {
            new SettingsAppLanguage() { LanguageCode = "en", Name = "English"},
            new SettingsAppLanguage() { LanguageCode = "ru", Name = "Русский"},
        };

        private SettingsAppTheme _selectedAppTheme;
        private SettingsAppLanguage _selectedAppLanguage;
        private bool _enableNotifications;
        private bool _enableExtendedNotifications;

        private bool _hideOnlineStatus;
        private bool _dontMarkMessagesAsRead;

        private bool _isRestartRequired;

        #region Commands

        public RelayCommand BuyInvisibleModeCommand { get; private set; }

        #endregion

        public List<SettingsAppTheme> AppThemes => _appThemes;

        public SettingsAppTheme SelectedAppTheme
        {
            get { return _selectedAppTheme; }
            set
            {
                if (Set(ref _selectedAppTheme, value))
                    ChangeTheme();
            }
        }

        public List<SettingsAppLanguage> AppLanguages => _appLanguages;

        public SettingsAppLanguage SelectedAppLanguage
        {
            get { return _selectedAppLanguage; }
            set
            {
                if (Set(ref _selectedAppLanguage, value))
                {
                    IsRestartRequired = true;

                    ApplicationLanguages.PrimaryLanguageOverride = value.LanguageCode;
                }
            }
        }

        public bool IsRestartRequired
        {
            get { return _isRestartRequired; }
            private set
            {
                if (Set(ref _isRestartRequired, value))
                {
                    var dialog = new MessageDialog(Localizator.String("SettingsRestartDialogContent"), Localizator.String("SettingsRestartDialogHeader"));
                    dialog.Commands.Add(new UICommand(Localizator.String("Close"), x => Application.Current.Exit()));
                    dialog.Commands.Add(new UICommand(Localizator.String("Cancel")));

                    dialog.ShowAsync();
                }
            }
        }

        public bool EnableNotifications
        {
            get { return _enableNotifications; }
            set
            {
                if (Set(ref _enableNotifications, value))
                {
                    AppSettings.EnableNotifications = value;

                    if (value)
                        PushHelper.Start();
                    else
                        PushHelper.Stop();
                }
            }
        }

        public bool EnableExtendedNotifications
        {
            get { return _enableExtendedNotifications; }
            set
            {
                if (Set(ref _enableExtendedNotifications, value))
                {
                    AppSettings.EnableExtendedNotifications = value;

                    PushHelper.Stop();
                    PushHelper.Start();
                }
            }
        }

        public bool HasAccessToInvisibleMode
        {
            get { return LicenseHelper.Instance.HasAccessToInvisibleMode; }
        }

        public bool HideOnlineStatus
        {
            get { return _hideOnlineStatus; }
            set
            {
                if (Set(ref _hideOnlineStatus, value))
                    AppSettings.HideOnlineStatus = value;
            }
        }

        public bool DontMarkMessagesAsRead
        {
            get { return _dontMarkMessagesAsRead; }
            set
            {
                if (Set(ref _dontMarkMessagesAsRead, value))
                    AppSettings.DontMarkMessagesAsRead = value;
            }
        }

        public SettingsViewModel()
        {
            //theme
            if (AppSettings.AppTheme != null)
            {
                switch (AppSettings.AppTheme)
                {
                    case ApplicationTheme.Light:
                        _selectedAppTheme = _appThemes.First(t => t.Value == ElementTheme.Light);
                        break;

                    case ApplicationTheme.Dark:
                        _selectedAppTheme = _appThemes.First(t => t.Value == ElementTheme.Dark);
                        break;
                }
            }
            else
                _selectedAppTheme = _appThemes.First(t => t.Value == ElementTheme.Default);

            //language
            var currentLanguage = ApplicationLanguages.PrimaryLanguageOverride;
            if (string.IsNullOrEmpty(currentLanguage))
                currentLanguage = Windows.System.UserProfile.GlobalizationPreferences.Languages.FirstOrDefault();

            _selectedAppLanguage = AppLanguages.FirstOrDefault(l => l.LanguageCode == currentLanguage);
            if (_selectedAppLanguage == null)
                _selectedAppLanguage = AppLanguages.First();

            _enableNotifications = AppSettings.EnableNotifications;
            _enableExtendedNotifications = AppSettings.EnableExtendedNotifications;

            if (HasAccessToInvisibleMode)
            {
                _hideOnlineStatus = AppSettings.HideOnlineStatus;
                _dontMarkMessagesAsRead = AppSettings.DontMarkMessagesAsRead;
            }
            else
            {
                AppSettings.HideOnlineStatus = false;
                AppSettings.DontMarkMessagesAsRead = false;
            }
        }

        protected override void InitializeCommands()
        {
            BuyInvisibleModeCommand = new RelayCommand(async () =>
            {
                await LicenseHelper.Instance.BuyInvisibleModeAccess();
                RaisePropertyChanged("HasAccessToInvisibleMode");
            });
        }

        private void ChangeTheme()
        {
            switch (SelectedAppTheme.Value)
            {
                case ElementTheme.Default:
                    AppSettings.AppTheme = null;
                    break;

                case ElementTheme.Light:
                    AppSettings.AppTheme = ApplicationTheme.Light;
                    break;

                case ElementTheme.Dark:
                    AppSettings.AppTheme = ApplicationTheme.Dark;
                    break;
            }

            JupiterApp.Current.NavigationService.Frame.RequestedTheme = SelectedAppTheme.Value;
            ThemeHelper.UpdateTitleBarColors();
        }
    }
}