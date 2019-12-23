using GalaSoft.MvvmLight.Messaging;

namespace Colibri.ViewModel.Messaging
{
    public class LoginStateChangedMessage : MessageBase
    {
        public bool IsLoggedIn { get; set; }
    }
}