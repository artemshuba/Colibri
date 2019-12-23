using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;
using Colibri.Helpers;
using Colibri.Services;
using Colibri.View;
using Colibri.ViewModel.Messaging;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Jupiter.Mvvm;
using VkLib.Core.Users;
using Windows.UI.Xaml;

namespace Colibri.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private VkProfile _currentUser;

        #region Commands

        public RelayCommand GoToContactsCommand { get; private set; }

        public RelayCommand SignOutCommand { get; private set; }

        public RelayCommand GoToSettingsCommand { get; private set; }

        #endregion

        public VkProfile CurrentUser
        {
            get { return _currentUser; }
            set { Set(ref _currentUser, value); }
        }

        public MainViewModel()
        {
            SubscribeMessages();
        }

        protected override void InitializeCommands()
        {
            GoToContactsCommand = new RelayCommand(() =>
            {
                Navigator.Content.Navigate(typeof(NewDialogView));
            });

            SignOutCommand = new RelayCommand(() =>
            {
                ServiceLocator.UserPresenceService.SetUserOffline();

                Messenger.Default.Send(new LoginStateChangedMessage() { IsLoggedIn = false });

                Navigator.Main.Navigate(typeof(LoginView), clearHistory: true);
            });

            GoToSettingsCommand = new RelayCommand(() =>
            {
                Navigator.Main.Navigate(typeof(SettingsView));
            });
        }

        public override async void OnNavigatedTo(Dictionary<string, object> parameters, NavigationMode mode)
        {
            if (CurrentUser == null && AppSettings.AccessToken != null && !AppSettings.AccessToken.HasExpired)
            {
                LoadCurrentUser();
                ServiceLocator.LongPollHandler.Start();
                if (AppSettings.EnableNotifications)
                    PushHelper.Start();
                ServiceLocator.UserPresenceService.SetUserOnline();

                if (((App)Application.Current).LaunchArgs != null)
                {
                    var args = ((App)Application.Current).LaunchArgs;
                    if (args != null && args.ContainsKey("uid"))
                    {
                        long uid = long.Parse(args["uid"]);

                        await Task.Delay(1);
                        Navigator.Content.Navigate(typeof(ChatView), new ConversationViewModel(uid));
                    }
                }

                try
                {
                    ServiceLocator.Vkontakte.Stats.TrackVisitor();
                }
                catch { }
            }
        }

        private async void LoadCurrentUser()
        {
            try
            {
                CurrentUser = await ServiceLocator.Vkontakte.Users.Get(AppSettings.AccessToken.UserId, "photo,photo_100");
            }
            catch (Exception ex)
            {
                //LoggingService.Log(ex);
                Debug.WriteLine(ex);
            }
        }

        private void SubscribeMessages()
        {
            Messenger.Default.Register<LoginStateChangedMessage>(this, OnLoginStateChanged);
        }

        private void OnLoginStateChanged(LoginStateChangedMessage message)
        {
            if (message.IsLoggedIn)
            {
                //PushHelper.Start();

                //InitializeLongPoll();
                LoadCurrentUser();

                try
                {
                    ServiceLocator.Vkontakte.Stats.TrackVisitor();
                }
                catch { }
            }
            else
            {
                ServiceLocator.LongPollHandler.Stop();
                PushHelper.Stop();
                AppSettings.AccessToken = null;
                ServiceLocator.Vkontakte.AccessToken = null;
                CurrentUser = null;
            }
        }
    }
}