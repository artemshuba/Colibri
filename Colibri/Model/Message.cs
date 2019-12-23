using Colibri.Helpers;
using Jupiter.Mvvm;
using VkLib.Core.Messages;
using VkLib.Core.Users;

namespace Colibri.Model
{
    public class Message : BindableBase
    {
        private VkMessage _messageContent;

        private bool _isSent = true;
        private bool _isFailed = false;

        public VkMessage MessageContent
        {
            get { return _messageContent; }
            set { Set(ref _messageContent, value); }
        }

        public VkProfile Sender { get; set; }

        public bool IsNew { get; set; } //используется только для анимации новых сообщений

        public bool IsRead
        {
            get { return _messageContent.IsRead; }
            set
            {
                _messageContent.IsRead = value;
                RaisePropertyChanged("IsRead");
                RaisePropertyChanged("MessageStatus");
            }
        }


        public bool IsSent
        {
            get { return _isSent; }
            set
            {
                _isSent = value;
                RaisePropertyChanged("IsSent");
                RaisePropertyChanged("MessageStatus");
            }
        }

        public bool IsFailed
        {
            get { return _isFailed; }
            set
            {
                _isFailed = value;
                RaisePropertyChanged("IsFailed");
                RaisePropertyChanged("MessageStatus");
            }
        }

        public string MessageStatus
        {
            get
            {
                if (_isFailed)
                    return Localizator.String("MessageStatusFailed");

                if (!_isSent)
                    return Localizator.String("MessageStatusSending");

                if (!IsRead)
                    return Localizator.String("MessageStatusUnread");

                return string.Empty;
            }
        }

        public Message(VkMessage message, VkProfile sender)
        {
            MessageContent = message;
            Sender = sender;
        }

        public string GetActionText()
        {
            if (string.IsNullOrEmpty(MessageContent.Action))
                return null;

            switch (MessageContent.Action)
            {
                case "chat_create":
                    return string.Format(Localizator.String("ChatServiceMessageChatCreated"), Sender?.Name, MessageContent.ActionText);
                case "chat_photo_update":
                    return string.Format(Localizator.String("ChatServiceMessagePhotoUpdated"), Sender?.Name);
                case "chat_photo_remove":
                    return string.Format(Localizator.String("ChatServiceMessagePhotoRemoved"), Sender?.Name);
                case "chat_kick_user":
                    return string.Format(Localizator.String("ChatServiceMessageKickUser"), Sender?.Name);
            }

            return null;
        }
    }
}
