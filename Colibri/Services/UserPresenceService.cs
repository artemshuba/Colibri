using System;
using Windows.UI.Xaml;
using VkLib;

namespace Colibri.Services
{
    public class UserPresenceService
    {
        private Vk _vk;
        private DispatcherTimer _onlineTimer;

        public UserPresenceService(Vk vk)
        {
            _vk = vk;

            _onlineTimer = new DispatcherTimer();
            _onlineTimer.Interval = TimeSpan.FromSeconds(15);
            _onlineTimer.Tick += OnlineTimerTick;

            Window.Current.VisibilityChanged += WindowVisibilityChanged;
        }

        public async void SetUserOnline()
        {
            try
            {
                if (_vk.AccessToken.HasExpired == true)
                    return;

                if (!AppSettings.HideOnlineStatus)
                    await _vk.Account.SetOnline();

                if (!_onlineTimer.IsEnabled)
                    _onlineTimer.Start();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to set user online");
            }
        }

        public async void SetUserOffline()
        {
            try
            {
                if (_vk.AccessToken.HasExpired == true)
                    return;

                if (!AppSettings.HideOnlineStatus)
                    await _vk.Account.SetOffline();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to set user offline");
            }
            finally
            {
                _onlineTimer.Stop();
            }
        }

        private void OnlineTimerTick(object sender, object e)
        {
            if (Window.Current.Visible)
            {
                SetUserOnline();
            }
            else
            {
                SetUserOffline();
            }
        }

        private void WindowVisibilityChanged(object sender, Windows.UI.Core.VisibilityChangedEventArgs e)
        {
            if (e.Visible && !_onlineTimer.IsEnabled)
                _onlineTimer.Start();
            else if (!e.Visible && _onlineTimer.IsEnabled)
                _onlineTimer.Stop();
        }
    }
}