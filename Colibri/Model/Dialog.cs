using Jupiter.Utils.Extensions;
using System.Collections.Generic;
using System.Linq;
using Colibri.Helpers;
using Jupiter.Mvvm;
using Jupiter.Utils.Helpers;
using VkLib.Core.Messages;
using VkLib.Core.Users;
using System.Globalization;

namespace Colibri.Model
{
    public class Dialog : BindableBase
    {
        private VkMessage _message;
        private User _user;
        private int _unreadCount;

        public VkMessage Message
        {
            get { return _message; }
            set
            {
                if (Set(ref _message, value))
                    RaisePropertyChanged("Preview");
            }
        }

        public User User
        {
            get { return _user; }
            set { Set(ref _user, value); }
        }

        /// <summary>
        /// Users (for multidialog)
        /// </summary>
        public List<VkProfile> Users { get; set; }

        public string Preview => GetPreview();

        public bool IsRead
        {
            get { return _message.IsRead; }
            set
            {
                if (_message.IsRead == value)
                    return;

                _message.IsRead = value;
                if (value)
                    UnreadCount = 0;
                RaisePropertyChanged();
            }
        }

        public int UnreadCount
        {
            get { return _unreadCount; }
            set { Set(ref _unreadCount, value); }
        }

        public Dialog()
        {

        }

        public Dialog(VkDialog dialog)
        {
            UnreadCount = dialog.Unread;
            _message = dialog.Message;
        }

        public Dialog(VkMessage message)
        {
            _message = message;
        }

        private string GetPreview()
        {
            if (Message == null)
                return string.Empty;

            string result = string.Empty;
            var locale = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

            if (!string.IsNullOrEmpty(Message.Body))
                result = Message.Body;
            else if (Message.Action != null)
            {
                result = new Message(Message, User.Profile).GetActionText();
            }
            else if (!Message.Attachments.IsNullOrEmpty())
            {
                var first = Message.Attachments[0];
                var count = Message.Attachments.Count;

                if (count > 1 && Message.Attachments.Any(a => a.Type != first.Type))
                    result = count + " " + StringHelper.LocalizeNumerals(count, Localizator.String("AttachmentsSingular"), Localizator.String("AttachmentsDual"), Localizator.String("AttachmentsPlural"), locale);
                else
                {
                    switch (first.Type)
                    {
                        case "audio":
                            result = count + " " + StringHelper.LocalizeNumerals(count, Localizator.String("AudiosSingular"), Localizator.String("AudiosDual"), Localizator.String("AudiosPlural"), locale);
                            break;

                        case "photo":
                            result = count + " " + StringHelper.LocalizeNumerals(count, Localizator.String("PhotosSingular"), Localizator.String("PhotosDual"), Localizator.String("PhotosPlural"), locale);
                            break;

                        case "sticker":
                            result = Localizator.String("DialogSticker");
                            break;

                        case "gift":
                            result = Localizator.String("DialogGift");
                            break;

                        case "link":
                            result = Localizator.String("DialogLink");
                            break;

                        case "doc":
                            result = count + " " + StringHelper.LocalizeNumerals(count, Localizator.String("DocumentsSingular"), Localizator.String("DocumentsDual"), Localizator.String("DocumentsPlural"), locale);
                            break;

                        case "video":
                            result = count + " " + StringHelper.LocalizeNumerals(count, Localizator.String("VideosSingular"), Localizator.String("VideosDual"), Localizator.String("VideosPlural"), locale);
                            break;

                        case "wall":
                            result = Localizator.String("DialogWallPost");
                            break;

                        default:
                            result = count + " " + StringHelper.LocalizeNumerals(count, Localizator.String("AttachmentsSingular"), Localizator.String("AttachmentsDual"), Localizator.String("AttachmentsPlural"), locale);
                            break;
                    }
                }
            }

            if (string.IsNullOrEmpty(result) && Message.Geo != null)
            {
                result = Localizator.String("DialogLocation");
            }

            if (!Message.ForwardMessages.IsNullOrEmpty())
            {
                result = Message.ForwardMessages.Count + " " + StringHelper.LocalizeNumerals(Message.ForwardMessages.Count, Localizator.String("DialogForwardedMessagesSingular"), Localizator.String("DialogForwardedMessagesDual"), Localizator.String("DialogForwardedMessagesPlural"), locale);
            }

            return result;
        }
    }
}
