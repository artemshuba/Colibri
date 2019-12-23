using System;
using Windows.Security.Authentication.Web;
using VkLib.Error;

namespace Colibri.Services
{
    public class ExceptionHandler
    {
        private bool _isValidating;

        public void Handle(Exception ex)
        {
            if (ex is VkNeedValidationException)
            {
                ValidateUser(((VkNeedValidationException)ex).RedirectUri);
            }
        }

        private async void ValidateUser(Uri redirectUri)
        {
            if (_isValidating)
                return;

            _isValidating = true;

            var result = await WebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.None, redirectUri, new Uri("https://oauth.vk.com/blank.html"));
            if (result.ResponseStatus == WebAuthenticationStatus.Success)
            {
                var token = ServiceLocator.Vkontakte.OAuth.ProcessAuth(new Uri(result.ResponseData));
                if (token != null && token.AccessToken != null && token.AccessToken.Token != null)
                {
                    token.AccessToken.ExpiresIn = DateTime.MaxValue;

                    ServiceLocator.Vkontakte.AccessToken = token.AccessToken;
                    AppSettings.AccessToken = token.AccessToken;
                }
            }

            _isValidating = false;
        }
    }
}