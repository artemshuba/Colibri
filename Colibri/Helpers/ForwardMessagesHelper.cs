using System.Collections.Generic;
using Colibri.Model;

namespace Colibri.Helpers
{
    public static class ForwardMessagesHelper
    {
        public static Dictionary<long, List<Message>> PendingForwardMessages { get; set; } = new Dictionary<long, List<Message>>();

        public static Message PendingForwardMessage { get; set; }
    }
}