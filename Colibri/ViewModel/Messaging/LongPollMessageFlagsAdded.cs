using VkLib.Core.Messages;

namespace Colibri.ViewModel.Messaging
{
    public class LongPollMessageFlagsAdded
    {
        public long MessageId { get; set; }

        public long UserId { get; set; }

        public VkLongPollMessageFlags Flags { get; set; }
    }
}