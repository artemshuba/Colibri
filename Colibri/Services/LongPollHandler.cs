using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Colibri.Helpers;
using Colibri.Model;
using Colibri.ViewModel;
using Colibri.ViewModel.Messaging;
using GalaSoft.MvvmLight.Messaging;
using VkLib;
using VkLib.Core.Messages;
using VkLib.Core.Users;

namespace Colibri.Services
{
    public class LongPollHandler
    {
        private Vk _vk;
        private int _failesCount = 0;
        private CancellationTokenSource _cancellationToken = new CancellationTokenSource();

        public LongPollHandler(Vk vk)
        {
            _vk = vk;
        }

        public async void Start()
        {
            try
            {
                await _vk.LongPollService.Start(_cancellationToken.Token, OnLongPollMessage);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);

                ServiceLocator.ExceptionHandler.Handle(ex);

                _failesCount++;
                if (_failesCount < 5)
                    Start();
            }
        }

        public void Stop()
        {
            _cancellationToken.Cancel();
            _cancellationToken = new CancellationTokenSource();

            ServiceLocator.Vkontakte.LongPollService.Stop();
        }

        private async void OnLongPollMessage(List<VkLongPollMessage> messages)
        {
            _failesCount = 0;

            foreach (var vkLongPollMessage in messages)
            {
                VkLongPollMessageFlags flags;
                long userId;
                string messageId;

                switch (vkLongPollMessage.Type)
                {
                    case VkLongPollMessageType.MessageAdd:
                        var newMessage = (VkMessage)vkLongPollMessage.Parameters["message"];
                        newMessage.Body = StringFormatter.HtmlDecode(newMessage.Body); //fix html entities
                        flags = (VkLongPollMessageFlags)vkLongPollMessage.Parameters["flags"];
                        var isOut = (flags & VkLongPollMessageFlags.Outbox) == VkLongPollMessageFlags.Outbox;
                        var user = !isOut ? await GetUser(newMessage.UserId) : ViewModelLocator.Main.CurrentUser;
                        newMessage.IsOut = isOut;
                        Messenger.Default.Send(new LongPollChatMessage() { Message = new Message(newMessage, user), Flags = flags });
                        break;

                    case VkLongPollMessageType.FriendOnline:
                        Messenger.Default.Send(new LongPollFriendOnlineStatusChangedMessage() { UserId = Math.Abs(long.Parse((string)vkLongPollMessage.Parameters["user_id"])), IsOnline = true });
                        break;

                    case VkLongPollMessageType.FriendOffline:
                        Messenger.Default.Send(new LongPollFriendOnlineStatusChangedMessage() { UserId = Math.Abs(long.Parse((string)vkLongPollMessage.Parameters["user_id"])), IsOnline = false });
                        break;

                    case VkLongPollMessageType.MessageFlagReset:
                        messageId = (string)vkLongPollMessage.Parameters["message_id"];
                        flags = (VkLongPollMessageFlags)vkLongPollMessage.Parameters["flags"];
                        userId = 0;
                        if (vkLongPollMessage.Parameters.ContainsKey("user_id"))
                            userId = (long)vkLongPollMessage.Parameters["user_id"];
                        Messenger.Default.Send(new LongPollMessageFlagsRemoved() { Flags = flags, MessageId = Math.Abs(long.Parse(messageId)), UserId = userId });
                        break;

                    case VkLongPollMessageType.MessageFlagSet:
                        messageId = (string)vkLongPollMessage.Parameters["message_id"];
                        flags = (VkLongPollMessageFlags)vkLongPollMessage.Parameters["flags"];
                        userId = 0;
                        if (vkLongPollMessage.Parameters.ContainsKey("user_id"))
                            userId = (long)vkLongPollMessage.Parameters["user_id"];
                        Messenger.Default.Send(new LongPollMessageFlagsAdded() { Flags = flags, MessageId = Math.Abs(long.Parse(messageId)), UserId = userId });
                        break;

                    case VkLongPollMessageType.DialogUserTyping:
                        userId = 0;
                        if (vkLongPollMessage.Parameters.ContainsKey("user_id"))
                            userId = (long)vkLongPollMessage.Parameters["user_id"];
                        Messenger.Default.Send(new LongPollUserStartedTypingMessage() { UserId = userId });
                        break;

                    case VkLongPollMessageType.MessageDelete:
                        messageId = (string)vkLongPollMessage.Parameters["message_id"];
                        Messenger.Default.Send(new LongPollMessageDeleted() { MessageId = long.Parse(messageId) });
                        break;
                }
            }
        }

        private async Task<VkProfile> GetUser(long userId)
        {
            try
            {
                return await ServiceLocator.Vkontakte.Users.Get(userId, "photo,online");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return null;
        }
    }
}