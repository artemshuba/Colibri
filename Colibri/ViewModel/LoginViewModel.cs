using System;
using Windows.Security.Authentication.Web;
using Windows.System;
using Colibri.Helpers;
using Colibri.Services;
using Colibri.ViewModel.Messaging;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Jupiter.Mvvm;
using VkLib.Auth;
using VkLib.Error;

namespace Colibri.ViewModel
{
    public class LoginViewModel : ViewModelBase
    {
        private string _login;
        private string _password;
        private bool _canLogin;
        private bool _canInput = true;

        private bool _isCaptchaVisible;
        private string _captchaImg;
        private string _captchaSid;
        private string _captchaKey;

        #region Commands

        public DelegateCommand LoginCommand { get; private set; }

        public RelayCommand RestorePasswordCommand { get; private set; }

        public RelayCommand SignUpCommand { get; private set; }

        #endregion

        public string Login
        {
            get { return _login; }
            set
            {
                Set(ref _login, value);
                UpdateCanLogin();
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                Set(ref _password, value);
                UpdateCanLogin();
            }
        }

        public bool CanLogin
        {
            get { return _canLogin; }
            set
            {
                Set(ref _canLogin, value);
            }
        }

        public bool CanInput
        {
            get { return _canInput; }
            set
            {
                Set(ref _canInput, value);
            }
        }

        public bool IsCaptchaVisible
        {
            get { return _isCaptchaVisible; }
            private set { Set(ref _isCaptchaVisible, value); }
        }

        public string CaptchaImg
        {
            get { return _captchaImg; }
            private set { Set(ref _captchaImg, value); }
        }

        public string CaptchaKey
        {
            get { return _captchaKey; }
            set { Set(ref _captchaKey, value); }
        }

        public LoginViewModel()
        {
            RegisterTasks("login");
        }

        protected override void InitializeCommands()
        {
            LoginCommand = new DelegateCommand(DoLogin);

            RestorePasswordCommand = new RelayCommand(async () =>
            {
                await Launcher.LaunchUriAsync(new Uri("http://vk.com/restore"));
            });

            SignUpCommand = new RelayCommand(async () =>
            {
                await Launcher.LaunchUriAsync(new Uri("http://vk.com"));
            });
        }

        private void UpdateCanLogin()
        {
            if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password))
                CanLogin = false;
            else
                CanLogin = true;
        }

        public async void DoLogin()
        {
            if (Operations["login"].IsWorking)
                return;

            var t = TaskStarted("login");
            CanInput = false;

            try
            {
                var token = await ServiceLocator.Vkontakte.Auth.Login(Login, Password, VkScopeSettings.CanAccessFriends |
                                                                                       VkScopeSettings.CanAccessGroups |
                                                                                       VkScopeSettings.CanAccessMessages |
                                                                                       VkScopeSettings.CanAccessWall |
                                                                                       VkScopeSettings.CanAccessVideos |
                                                                                       VkScopeSettings.CanAccessPhotos |
                                                                                       VkScopeSettings.CanAccessDocs |
                                                                                       VkScopeSettings.CanAccessAudios,
                    _captchaSid, _captchaKey);

                if (token != null)
                {
                    AppSettings.AccessToken = token;
                }

                Navigator.Main.Navigate(typeof(MainPage), clearHistory: true);
                Messenger.Default.Send(new LoginStateChangedMessage() { IsLoggedIn = true });
            }
            catch (VkCaptchaNeededException ex)
            {
                Logger.Error(ex, "Unable to login. Captcha needed.");

                IsCaptchaVisible = true;
                _captchaSid = ex.CaptchaSid;
                CaptchaImg = ex.CaptchaImg;
            }
            catch (VkInvalidClientException ex)
            {
                Logger.Error(ex, "Unable to login. Invalid client.");

                t.Error = Localizator.String("Errors/LoginInvalidClientError");
            }
            catch (VkNeedValidationException ex)
            {
                Logger.Error(ex, "Validation requred");

                ValidateUser(ex.RedirectUri);

                UpdateCanLogin();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to login.");

                t.Error = Localizator.String("Errors/LoginCommonError");
            }

            t.Finish();

            CanInput = true;
        }

        private async void ValidateUser(Uri redirectUri)
        {
            var result = await WebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.None, redirectUri, new Uri("https://oauth.vk.com/blank.html"));
            if (result.ResponseStatus == WebAuthenticationStatus.Success)
            {
                var token = ServiceLocator.Vkontakte.OAuth.ProcessAuth(new Uri(result.ResponseData));
                if (token != null && token.AccessToken != null && token.AccessToken.Token != null)
                {
                    token.AccessToken.ExpiresIn = DateTime.MaxValue;

                    ServiceLocator.Vkontakte.AccessToken = token.AccessToken;
                    AppSettings.AccessToken = token.AccessToken;

                    Navigator.Main.Navigate(typeof(MainPage), clearHistory: true);
                    Messenger.Default.Send(new LoginStateChangedMessage() { IsLoggedIn = true });
                }
            }
        }
    }
}