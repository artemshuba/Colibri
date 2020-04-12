using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Networking.PushNotifications;
using Colibri.Services;
using Colibri.ViewModel.Messaging;
using GalaSoft.MvvmLight.Messaging;
using Jupiter.Utils.Helpers;
using VkLib.Core.Push;

namespace Colibri.Helpers
{
    public static class PushHelper
    {
        private static PushNotificationChannel _notificationChannel;
        private static readonly string BACKGROUND_ENTRY_POINT = typeof(BackgroundTasks.PushNotificationBackgroundTask).FullName;

        public static async void Start()
        {
            try
            {
                _notificationChannel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();

                _notificationChannel.PushNotificationReceived += PushNotificationReceived;

                await ServiceLocator.Vkontakte.Account.UnregisterDevice(DeviceHelper.GetDeviceId());

                //TODO временно отключено, т.к. сервак лежит
                string token = /*AppSettings.EnableExtendedNotifications
                    ? "http://colibri.mswin.me/api/push?channel=" + _notificationChannel.Uri
                    :*/ _notificationChannel.Uri;

                var result = await ServiceLocator.Vkontakte.Account.RegisterDevice(token, deviceId: DeviceHelper.GetDeviceId(), systemVersion: DeviceHelper.GetOsVersion(),
                           settings: new VkPushSettings()
                           {
                               Messages = new VkPushMessageSettings() { IsEnabled = true },
                               Chat = new VkPushMessageSettings() { IsEnabled = true }
                           });

                if (result)
                {
                    await RegisterBackgroundTask();
                    Debug.WriteLine("Push notifications registered");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to register device for push notifications");

                ServiceLocator.ExceptionHandler.Handle(ex);
            }

        }

        public static async void Stop()
        {
            if (_notificationChannel != null)
            {
                _notificationChannel.PushNotificationReceived -= PushNotificationReceived;
                _notificationChannel.Close();
                _notificationChannel = null;
            }

            try
            {
                var result = await ServiceLocator.Vkontakte.Account.UnregisterDevice(DeviceHelper.GetDeviceId());
                if (result)
                    Debug.WriteLine("Push notifications unregistered");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to unregister device for push notifications");
            }

        }

        private static void PushNotificationReceived(PushNotificationChannel sender, PushNotificationReceivedEventArgs e)
        {
            Debug.WriteLine("Push notification received: " + e.NotificationType);

            Messenger.Default.Send(new PushNotificationReceivedMessage() { Args = e });
        }

        private static async Task<bool> RegisterBackgroundTask()
        {
            // Unregister any previous exising background task
            UnregisterBackgroundTask();

            // Request access
            BackgroundAccessStatus status = await BackgroundExecutionManager.RequestAccessAsync();

            // If denied
            if (status == BackgroundAccessStatus.DeniedBySystemPolicy && status == BackgroundAccessStatus.DeniedByUser)
                return false;

            // Construct the background task
            BackgroundTaskBuilder builder = new BackgroundTaskBuilder()
            {
                Name = BACKGROUND_ENTRY_POINT,
                TaskEntryPoint = BACKGROUND_ENTRY_POINT
            };

            // Set trigger for Toast History Changed
            builder.SetTrigger(new ToastNotificationActionTrigger());

            // And register the background task
            BackgroundTaskRegistration registration = builder.Register();

            // And then listen for when the background task updates progress (it passes the toast change type)
            //registration.Completed += BackgroundTask_Completed;

            return true;
        }

        private static void UnregisterBackgroundTask()
        {
            var task = BackgroundTaskRegistration.AllTasks.Values.FirstOrDefault(i => i.Name.Equals(BACKGROUND_ENTRY_POINT));

            if (task != null)
                task.Unregister(true);
        }
    }
}
