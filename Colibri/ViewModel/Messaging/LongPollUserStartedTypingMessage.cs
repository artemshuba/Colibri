using GalaSoft.MvvmLight.Messaging;

namespace Colibri.ViewModel.Messaging
{
    public class LongPollUserStartedTypingMessage : MessageBase
    {
        public long UserId { get; set; }
    }
}