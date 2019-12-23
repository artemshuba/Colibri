using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Colibri.Helpers;
using Colibri.View;
using Colibri.ViewModel.Messaging;
using GalaSoft.MvvmLight.Messaging;
using VkLib;
using VkLib.Core.Messages;
using VkLib.Core.Users;
using VkLib.Error;

namespace Colibri.Services
{
    public class UserService
    {
        private const string FIELDS = "photo,online,last_seen";

        private Vk _vk;

        public UserService(Vk vk)
        {
            _vk = vk;
        }

        public async Task<VkProfile> GetById(long userId)
        {
            try
            {
                return await ServiceLocator.Vkontakte.Users.Get(userId, FIELDS);
            }
            catch (VkInvalidTokenException)
            {
                Messenger.Default.Send(new LoginStateChangedMessage() { IsLoggedIn = false });

                Navigator.Main.Navigate(typeof(LoginView), clearHistory: true);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to load user with id " + userId);
            }

            return null;
        }


        public async Task<List<VkProfile>> GetById(List<long> userIds)
        {
            try
            {
                var result = await ServiceLocator.Vkontakte.Users.Get(userIds.Select(i => i.ToString()), FIELDS);
                if (result != null)
                    return result.Items;
            }
            catch (VkInvalidTokenException)
            {
                Messenger.Default.Send(new LoginStateChangedMessage() { IsLoggedIn = false });

                Navigator.Main.Navigate(typeof(LoginView), clearHistory: true);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to load users with ids " + string.Join(",", userIds.Select(i => i.ToString())));
            }

            return null;
        }

        public async Task<List<VkProfile>> GetChatUsers(long chatId)
        {
            try
            {
                var result = await ServiceLocator.Vkontakte.Messages.GetChatUsers(chatId, fields: FIELDS);
                return result;
            }
            catch (VkInvalidTokenException)
            {
                Messenger.Default.Send(new LoginStateChangedMessage() { IsLoggedIn = false });

                Navigator.Main.Navigate(typeof(LoginView), clearHistory: true);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to load chat users with id " + chatId);
            }

            return null;
        }

        public async Task<VkConversation> GetChat(long chatId)
        {
            try
            {
                var result = await ServiceLocator.Vkontakte.Messages.GetChat(chatId, fields: FIELDS);
                return result;
            }
            catch (VkInvalidTokenException)
            {
                Messenger.Default.Send(new LoginStateChangedMessage() { IsLoggedIn = false });

                Navigator.Main.Navigate(typeof(LoginView), clearHistory: true);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to load chat with id " + chatId);
            }

            return null;
        }

        public async Task<List<VkProfile>> SearchHints(string query, int count = 10)
        {
            var result = await _vk.Execute.SearchHints(query, fields: FIELDS, count: count, searchGlobal: true, filters: "friends");
            return result;
        }
    }
}