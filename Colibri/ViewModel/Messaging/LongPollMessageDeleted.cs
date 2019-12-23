using GalaSoft.MvvmLight.Messaging;

namespace Colibri.ViewModel.Messaging
{
    public class LongPollMessageDeleted : MessageBase
    {
        public long MessageId { get; set; }
    }
}