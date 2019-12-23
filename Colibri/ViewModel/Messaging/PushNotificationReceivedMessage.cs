using Windows.Networking.PushNotifications;
using GalaSoft.MvvmLight.Messaging;

namespace Colibri.ViewModel.Messaging
{
    public class PushNotificationReceivedMessage : MessageBase
    {
        public PushNotificationReceivedEventArgs Args { get; set; }
    }
}
