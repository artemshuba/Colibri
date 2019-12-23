using GalaSoft.MvvmLight.Messaging;

namespace Colibri.ViewModel.Messaging
{
    public class NewDialogStartedMessage : MessageBase
    {
        public long UserId { get; set; }
    }
}
