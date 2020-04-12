using System;
using System.Collections.Generic;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Globalization;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Colibri.Helpers;
using Colibri.Services;
using Colibri.View;
using Colibri.ViewModel.Messaging;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using Jupiter.Utils.Extensions;
using MetroLog;
using Jupiter.Application;

namespace Colibri
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : JupiterApp
    {
        public Dictionary<string, string> LaunchArgs { get; set; }

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();

            if (AppSettings.AppTheme != null)
                RequestedTheme = AppSettings.AppTheme.Value;

#if DEBUG
            // setup the global crash handler...
            GlobalCrashHandler.Configure();
#endif
        }

        public override void OnStart(StartKind startKind, IActivatedEventArgs args)
        {
            var launchArgs = ExtractArgumentsString(args);
            if (launchArgs != null)
            {
                Logger.Info("Launch args: " + launchArgs);
                LaunchArgs = launchArgs.ParseQueryString();
            }

            DispatcherHelper.Initialize();
            LicenseHelper.Instance.Initialize();

            if (!string.IsNullOrEmpty(ApplicationLanguages.PrimaryLanguageOverride))
                ServiceLocator.Vkontakte.Language = ApplicationLanguages.PrimaryLanguageOverride;

            if (startKind == StartKind.Activate && args.PreviousExecutionState == ApplicationExecutionState.Running)
            {
                Window.Current.Activate();

                if (LaunchArgs != null && LaunchArgs.ContainsKey("uid"))
                {
                    long uid = long.Parse(LaunchArgs["uid"]);

                    Messenger.Default.Send(new GoToDialogMessage() { UserId = uid });
                }
            }
            else
            {
                Logger.AppStart();

                var appView = ApplicationView.GetForCurrentView();
                //appView.TitleBar.BackgroundColor = ((SolidColorBrush)Resources["ConversationOutboxMessageForegroundBrush"]).Color;
                //appView.TitleBar.InactiveBackgroundColor = Colors.Transparent;//appView.TitleBar.BackgroundColor;
                var c = this.RequestedTheme == ApplicationTheme.Light ? Colors.White : Colors.Black;
                var cf = this.RequestedTheme == ApplicationTheme.Light ? Colors.Black : Colors.White;
                appView.TitleBar.ButtonBackgroundColor = Color.FromArgb(0, c.R, c.G, c.B); //appView.TitleBar.BackgroundColor;
                appView.TitleBar.ButtonInactiveBackgroundColor = appView.TitleBar.ButtonBackgroundColor;
                appView.TitleBar.ButtonForegroundColor = cf;
                //appView.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;//appView.TitleBar.BackgroundColor;
                //appView.TitleBar.ForegroundColor = Colors.White;

                CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;

                if (AppSettings.AccessToken == null || AppSettings.AccessToken.HasExpired)
                    NavigationService.Navigate(typeof(LoginView));
                else
                {
                    ServiceLocator.Vkontakte.AccessToken = AppSettings.AccessToken;
                    NavigationService.Navigate(typeof(MainPage));
                }
            }

            TileHelper.ClearTile();
        }

        public override void OnUnhandledException(Windows.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            Logger.Fatal(e.Exception, e.Message);
        }

        public override void OnSuspending(SuspendingEventArgs e)
        {
            Logger.AppSuspeinding();
        }

        private string ExtractArgumentsString(IActivatedEventArgs args)
        {
            if (args is ILaunchActivatedEventArgs)
                return ((ILaunchActivatedEventArgs)args).Arguments;
            else if (args is IToastNotificationActivatedEventArgs)
                return ((IToastNotificationActivatedEventArgs)args).Argument;

            return null;
        }
    }
}