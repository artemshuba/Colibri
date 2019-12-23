using Colibri.Model;
using GalaSoft.MvvmLight.Messaging;
using VkLib.Core.Messages;

namespace Colibri.ViewModel.Messaging
{
    public class LongPollChatMessage : MessageBase
    {
        public Message Message { get; set; }

        public VkLongPollMessageFlags Flags { get; set; }
    }
}
