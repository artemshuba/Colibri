using System;
using System.Collections.Generic;
using Colibri.Model;
using Colibri.ViewModel;
using Jupiter.Collections;
using VkLib.Core.Attachments;
using VkLib.Core.Messages;
using VkLib.Core.Users;

namespace Colibri.DebugData
{
    public class DebugDialogsViewModel : DialogsViewModel
    {
#if DEBUG || MOCK
        public DebugDialogsViewModel()
        {
            Dialogs = new IncrementalLoadingCollection<Dialog>()
            {
                new Dialog(new VkMessage()
                {
                    Body = "Привет! Как дела?",
                    Date = DateTime.Now.AddDays(-2),
                    IsRead = true
                })
                {
                    User = new User(new VkProfile()
                    {
                        FirstName = "Vitaliy",
                        LastName = "Tihonov",
                        Photo = "https://s3.amazonaws.com/uifaces/faces/twitter/sauro/128.jpg",
                        IsOnline = true
                    })
                },

                new Dialog(new VkMessage()
                {
                    Body = "Да, мне кажется, это оптимальный вариант",
                    Date = DateTime.Now.AddDays(-1),
                    IsRead = true
                })
                {
                    User = new User(new VkProfile()
                    {
                        FirstName = "Semen",
                        LastName = "Pestov",
                        Photo = "https://s3.amazonaws.com/uifaces/faces/twitter/jsa/128.jpg",
                    })
                },

                new Dialog(new VkMessage()
                {
                    Body = "Ок, я тебе вечером напишу.",
                    Date = DateTime.Now,
                    IsRead = true
                })
                {
                    User = new User(new VkProfile()
                    {
                        FirstName = "Irina",
                        LastName = "Pahomova",
                        Photo = "https://s3.amazonaws.com/uifaces/faces/twitter/pixeliris/128.jpg",
                        IsOnline = true
                    })
                },

                new Dialog(new VkMessage()
                {
                    Body = "Дело в том, что нам важно успеть до следующей недели",
                    IsRead = true,
                    Date = DateTime.Now.AddMonths(-2)
                })
                {
                    User = new User(new VkProfile()
                    {
                        FirstName = "Victoria",
                        LastName = "Davidova",
                        Photo = "https://s3.amazonaws.com/uifaces/faces/twitter/nuraika/128.jpg",
                        IsOnlineMobile = true
                    })
                },

                new Dialog(new VkMessage()
                {
                    Body = "Привет, как успехи?",

                    Date = DateTime.Now.AddMonths(-2),
                    IsOut = true,
                    IsRead = true
                })
                {
                    User = new User(new VkProfile()
                    {
                        FirstName = "Pavel",
                        LastName = "Belousov",
                        Photo = "https://s3.amazonaws.com/uifaces/faces/twitter/dustinlamont/128.jpg",
                        IsOnlineMobile = true,
                    })
                },

                new Dialog(new VkMessage()
                {
                    Attachments = new List<VkAttachment>()
                    {
                        new VkAudioAttachment(),
                         new VkAudioAttachment()
                    },
                    Date = DateTime.Now.AddMonths(-2),
                    IsOut = false,
                    IsRead = true
                })
                {
                    User = new User(new VkProfile()
                    {
                        FirstName = "Lilia",
                        LastName = "Evseeva",
                        Photo = "https://s3.amazonaws.com/uifaces/faces/twitter/madysondesigns/128.jpg",
                        IsOnlineMobile = true,
                    })
                },

                new Dialog(new VkMessage()
                {
                    Attachments = new List<VkAttachment>()
                    {
                        new VkPhotoAttachment()
                    },

                    Date = DateTime.Now.AddMonths(-2),
                    IsOut = true,
                    IsRead = true
                })
                {
                    User = new User(new VkProfile()
                    {
                        FirstName = "Pavel",
                        LastName = "Durov",
                        Photo = "https://s3.amazonaws.com/uifaces/faces/twitter/tonychester/128.jpg",
                        IsOnlineMobile = true,
                    })
                },

                new Dialog(new VkMessage()
                {
                    Body = "Добрый вечер, интересует такой вопрос: возможно ли, чтобы название песни выводилось в текстовый",

                    Date = DateTime.Now.AddMonths(-2),
                    IsOut = false,
                    IsRead = true
                })
                {
                    User = new User(new VkProfile()
                    {
                        FirstName = "Alexandra",
                        LastName = "Krasilnikova",
                        Photo = "https://s3.amazonaws.com/uifaces/faces/twitter/allisongrayce/128.jpg",
                        IsOnlineMobile = true,
                    })
                },

                new Dialog(new VkMessage()
                {
                    Body = "ну да, я это и имею ввиду",

                    Date = DateTime.Now.AddMonths(-2),
                    IsOut = true,
                    IsRead = true
                })
                {
                    User = new User(new VkProfile()
                    {
                        FirstName = "Anfisa",
                        LastName = "Petrova",
                        Photo = "https://s3.amazonaws.com/uifaces/faces/twitter/stylecampaign/128.jpg",
                        IsOnlineMobile = true,
                    })
                },

            };
        }
        
#endif
    }
}
