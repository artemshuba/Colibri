using System.Collections.Generic;
using Colibri.Helpers;
using Colibri.Model;
using Colibri.ViewModel;
using VkLib.Core.Attachments;
using VkLib.Core.Messages;
using VkLib.Core.Users;

namespace Colibri.DebugData
{
    public class DebugConversationViewModel : ConversationViewModel
    {
#if DEBUG || MOCK
        public DebugConversationViewModel() : this(null)
        {

        }

        public DebugConversationViewModel(Dialog d) : base(d)
        {
            Dialog = new Dialog(new VkMessage()
            {

            })
            {
                User = new User(new VkProfile()
                {
                    FirstName = "Vitaliy",
                    LastName = "Tihonov",
                    Photo = "https://s3.amazonaws.com/uifaces/faces/twitter/sauro/128.jpg"
                })
            };

            Messages = new IncrementalTopLoadingCollection<Message>()
            {
              new Message(new VkMessage()
                    {
                        Body = "Привет! Покидай фоток каких-нибудь",
                        IsOut = true,
                        IsRead = true
                    },  Dialog.User.Profile),
                    new Message(new VkMessage()
                    {
                        Attachments = new List<VkAttachment>()
                        {
                            new VkPhotoAttachment() {Source = "http://www.viajararusia.ru/assets/admin/uploads/tours/1.jpg", Width = 800, Height = 533},
                            new VkPhotoAttachment() {Source = "http://cdn.images.express.co.uk/img/dynamic/25/590x/St-Petersburg-599569.jpg", Width = 550, Height = 366},
                            new VkPhotoAttachment() {Source = "https://lonelyplanetimages.imgix.net/mastheads/stock-photo-canals-of-st-petersburg-82408857.jpg?sharp=10&vib=20&w=1200", Width = 800, Height = 533}

                        },
                        IsOut = false,
                        IsRead = true
                    },  Dialog.User.Profile),
                    new Message(new VkMessage()
                    {
                        Body = "Еще нужно?",
                        IsOut = false,
                        IsRead = true
                    },  Dialog.User.Profile),
                    new Message(new VkMessage()
                    {
                        Body = "Нет, отлично, спасибо!",
                        IsOut = true,
                        IsRead = true
                    },  Dialog.User.Profile),
                    new Message(new VkMessage()
                    {
                        Body = "Хорошо смотрится. А темная тема будет?",
                        IsOut = false,
                        IsRead = true
                    },  Dialog.User.Profile),
                     new Message(new VkMessage()
                    {
                        Body = "Конечно",
                        IsOut = true
                    },  Dialog.User.Profile),
            };
        }
#endif
    }
}
