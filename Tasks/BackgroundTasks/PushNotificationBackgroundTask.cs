using System;
using System.Diagnostics;
using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;
using Jupiter.Services.Settings;
using Jupiter.Utils.Extensions;
using VkLib;
using VkLib.Core.Auth;

namespace BackgroundTasks
{
    public sealed class PushNotificationBackgroundTask : IBackgroundTask
    {
        private const string VKAPP_ID = "3059212";
        private const string VKAPP_SECRET = "CEPu3hrA9DK5Ur6pBbia";

        private static Vk Vkontakte { get; } = new Vk(VKAPP_ID, VKAPP_SECRET, "5.38");

        private static VkAccessToken AccessToken
        {
            get { return SettingsService.Roaming.Get<VkAccessToken>(); }
        }

        private static bool DontMarkMessagesAsRead
        {
            get { return SettingsService.Local.Get(defaultValue: false); }
        }

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            Debug.WriteLine("Push Background task started");

            var details = taskInstance.TriggerDetails as ToastNotificationActionTriggerDetail;
            if (details == null)
            {
                return;
            }

            Vkontakte.AccessToken = AccessToken;

            string arguments = details.Argument;
            Debug.WriteLine(arguments);


            foreach (var value in details.UserInput)
            {
                Debug.WriteLine(value.Key + " - " + value.Value);
            }

            if (!string.IsNullOrEmpty(arguments))
            {
                var launchArgs = arguments.ParseQueryString();
                if (launchArgs != null)
                {
                    if (launchArgs["action"] == "reply" && launchArgs.ContainsKey("uid") && details.UserInput.ContainsKey("message"))
                    {
                        long uid = long.Parse(launchArgs["uid"]);
                        bool isChat = uid > 2000000000;

                        var message = details.UserInput["message"].ToString();

                        var deferral = taskInstance.GetDeferral();

                        try
                        {
                            var result = await Vkontakte.Messages.Send(!isChat ? uid : 0, isChat ? uid - 2000000000 : 0, message);
                            Debug.WriteLine("result: " + result);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex);
                        }

                        deferral.Complete();
                    }
                }
            }
        }
    }
}