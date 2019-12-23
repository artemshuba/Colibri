using GalaSoft.MvvmLight.Messaging;

namespace Colibri.ViewModel.Messaging
{
    public class LongPollFriendOnlineStatusChangedMessage : MessageBase
    {
        public long UserId { get; set; }

        public bool IsOnline { get; set; }
    }
}
