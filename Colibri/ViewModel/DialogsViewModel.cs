using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;
using Colibri.Helpers;
using Colibri.Model;
using Colibri.Services;
using Colibri.View;
using Colibri.ViewModel.Messaging;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Jupiter.Collections;
using Jupiter.Mvvm;
using Jupiter.Utils.Extensions;
using Jupiter.Utils.Helpers;
using VkLib.Core.Messages;
using VkLib.Error;
using Windows.UI.Xaml;

namespace Colibri.ViewModel
{
    public class DialogsViewModel : ViewModelBase
    {
        internal const string USER_FIELDS = "photo_50,last_seen,online,sex";

        private IncrementalLoadingCollection<Dialog> _dialogs;
        private Dialog _selectedDialog;

        private int _totalCount;

        #region Commands

        public RelayCommand<Dialog> GoToDialogCommand { get; private set; }

        public RelayCommand RefreshCommand { get; private set; }

        #endregion

        public IncrementalLoadingCollection<Dialog> Dialogs
        {
            get { return _dialogs; }
            protected set { Set(ref _dialogs, value); }
        }

        public Dialog SelectedDialog
        {
            get { return _selectedDialog; }
            set { Set(ref _selectedDialog, value); }
        }

        public DialogsViewModel()
        {
            RegisterTasks("dialogs");

            SubscribeMessages();
        }

        ~DialogsViewModel()
        {
            UnsubscribeMessages();
        }

        public override void OnNavigatedTo(Dictionary<string, object> parameters, NavigationMode mode)
        {
            base.OnNavigatedTo(parameters, mode);

#if !MOCK
            if (mode == NavigationMode.New || Dialogs.IsNullOrEmpty())
            {
                Dialogs = null;
                Load();
            }
#endif
        }

        protected override void InitializeCommands()
        {
            GoToDialogCommand = new RelayCommand<Dialog>(GoToDialog);

            RefreshCommand = new RelayCommand(() =>
            {
                Dialogs = null;
                SelectedDialog = null;
                Load();
            });
        }

        private async void Load()
        {
            var t = TaskStarted("dialogs");

            try
            {
                var vkDialogs = await ServiceLocator.Vkontakte.Messages.GetDialogs();
                if (!vkDialogs.Items.IsNullOrEmpty())
                {
                    var dialogs = await ProcessDialogs(vkDialogs.Items);
                    _totalCount = vkDialogs.TotalCount;

                    Dialogs = new IncrementalLoadingCollection<Dialog>(dialogs);
                    Dialogs.HasMoreItemsRequested = () => _totalCount > Dialogs.Count;
                    Dialogs.OnMoreItemsRequested = LoadMoreDialogs;

                    if (((App)Application.Current).LaunchArgs != null)
                    {
                        var args = ((App)Application.Current).LaunchArgs;
                        if (args != null && args.ContainsKey("uid"))
                        {
                            long uid = long.Parse(args["uid"]);

                            var activeDialog = Dialogs.FirstOrDefault(d => d.User.Profile.Id == uid);
                            SelectedDialog = activeDialog;
                        }
                    }
                }
                else
                {
                    t.Error = Localizator.String("Errors/DialogsEmptyError");
                }
            }
            catch (VkInvalidTokenException)
            {
                Messenger.Default.Send(new LoginStateChangedMessage() { IsLoggedIn = false });

                Navigator.Main.Navigate(typeof(LoginView), clearHistory: true);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to load dialogs");

                t.Error = Localizator.String("Errors/DialogsCommonError");
            }

            t.Finish();
        }

        private async Task<List<Dialog>> LoadMoreDialogs(CancellationToken token, uint count)
        {
            try
            {
                var vkDialogs = await ServiceLocator.Vkontakte.Messages.GetDialogs(offset: Dialogs.Count, count: (int)count);
                if (!vkDialogs.Items.IsNullOrEmpty())
                {
                    TaskFinished("dialogs"); //таска может быть активна, тогда LoadingIndicator будет накладываться на список
                    var dialogs = await ProcessDialogs(vkDialogs.Items);

                    _totalCount = vkDialogs.TotalCount;

                    return dialogs;
                }
            }
            catch (VkInvalidTokenException)
            {
                Messenger.Default.Send(new LoginStateChangedMessage() { IsLoggedIn = false });

                Navigator.Main.Navigate(typeof(LoginView), clearHistory: true);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to load more dialogs");
            }

            return null;
        }

        private async Task<List<Dialog>> ProcessDialogs(List<VkDialog> vkDialogs)
        {
            var dialogs = new List<Dialog>();

            var userIds = vkDialogs.Select(d =>
            {
                if (d.Message.ChatUsers == null)
                    return d.Message.UserId.ToString();
                else
                {
                    var ids = d.Message.ChatUsers.Select(u => u).ToList();
                    ids.Add(d.Message.UserId); //id отправителя в беседе
                    var r = string.Join(",", ids.Distinct());
                    return r + "," + AppSettings.AccessToken?.UserId; //добавляем текущего юзера в списку юзеров чата
                }
            }).ToList();

            try
            {
                var users = await ServiceLocator.Vkontakte.Users.Get(userIds, USER_FIELDS);
                if (users != null)
                {
                    foreach (var vkDialog in vkDialogs)
                    {
                        var d = new Dialog(vkDialog);
                        if (vkDialog.Message.ChatUsers != null && vkDialog.Message.ChatUsers.Count > 0)
                        {
                            d.Users = users.Items.Where(x => vkDialog.Message.ChatUsers.Contains(x.Id)).ToList();
                        }

                        var u = users.Items.FirstOrDefault(x => vkDialog.Message.UserId == x.Id);
                        if (u != null)
                            d.User = new User(u);
                        else
                        {
                            Logger.Info("Found possible dialog with society " + vkDialog.Message.UserId);
                            //TODO пока не поддерживаются диалоги с сообществами, пропусками их
                            continue;
                        }

                        dialogs.Add(d);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to get dialog users");
            }

            return dialogs;
        }

        private void GoToDialog(Dialog dialog)
        {
            if (!DeviceHelper.IsMobile() && (_selectedDialog == dialog && Navigator.Content.FrameFacade.Content is ChatView))
                return;

            Navigator.Content.Navigate(typeof(ChatView), new ConversationViewModel(dialog));
        }

        #region Message interception

        private void SubscribeMessages()
        {
            Messenger.Default.Register<LongPollChatMessage>(this, OnChatMessage);
            Messenger.Default.Register<LongPollFriendOnlineStatusChangedMessage>(this, OnFriendOnlineStatusChanged);
            Messenger.Default.Register<LongPollMessageFlagsRemoved>(this, OnMessageFlagsRemoved);
            Messenger.Default.Register<LongPollMessageFlagsAdded>(this, OnMessageFlagsAdded);

            Messenger.Default.Register<NewDialogStartedMessage>(this, OnNewDialogStarted);
            Messenger.Default.Register<GoToDialogMessage>(this, OnGoToDialogMessage);
            Messenger.Default.Register<LoginStateChangedMessage>(this, OnLoginStateChanged);
        }

        private void UnsubscribeMessages()
        {
            Messenger.Default.Unregister<LongPollChatMessage>(this, OnChatMessage);
            Messenger.Default.Unregister<LongPollFriendOnlineStatusChangedMessage>(this, OnFriendOnlineStatusChanged);
            Messenger.Default.Unregister<LongPollMessageFlagsRemoved>(this, OnMessageFlagsRemoved);
            Messenger.Default.Unregister<LongPollMessageFlagsAdded>(this, OnMessageFlagsAdded);

            Messenger.Default.Unregister<NewDialogStartedMessage>(this, OnNewDialogStarted);
            Messenger.Default.Unregister<GoToDialogMessage>(this, OnGoToDialogMessage);
            Messenger.Default.Unregister<LoginStateChangedMessage>(this, OnLoginStateChanged);
        }

        private async void OnChatMessage(LongPollChatMessage message)
        {
            if (Dialogs == null)
                Dialogs = new IncrementalLoadingCollection<Dialog>();

            bool isChat = message.Message.MessageContent.ChatId != 0;

            var dialog = Dialogs?.FirstOrDefault(d => !isChat ? d.User?.Profile.Id == message.Message.MessageContent.UserId : d.Message.ChatId == message.Message.MessageContent.ChatId);
            if (dialog != null)
            {
                bool isSelected = _selectedDialog == dialog;
                dialog.Message = message.Message.MessageContent;
                if (isChat)
                    dialog.User = new User(message.Message.Sender);

                var dialogIndex = Dialogs.IndexOf(dialog);
                if (dialogIndex != 0)
                {
                    Dialogs.RemoveAt(dialogIndex);
                    Dialogs.Insert(0, dialog);
                    //Dialogs.Move(dialogIndex, 0); //if it's not top dialog in the list, move it
                }
                else
                    Dialogs[dialogIndex] = dialog; //notifying UI to update whole model (trigger templateselector)

                if (isSelected)
                    SelectedDialog = dialog;
            }
            else
            {
                dialog = new Dialog(message.Message.MessageContent);

                var user = await ServiceLocator.UserService.GetById(message.Message.MessageContent.UserId);
                if (user != null)
                {
                    dialog.User = new User(user);

                    Dialogs.Insert(0, dialog);
                }
            }

            dialog.UnreadCount++;
        }

        private void OnFriendOnlineStatusChanged(LongPollFriendOnlineStatusChangedMessage message)
        {
            var dialog = Dialogs?.FirstOrDefault(d => d.User?.Profile.Id == message.UserId);
            if (dialog != null && dialog.User != null)
            {
                dialog.User.IsOnline = message.IsOnline;
            }
        }

        private void OnMessageFlagsRemoved(LongPollMessageFlagsRemoved message)
        {
            long uid = message.UserId;
            var dialog = Dialogs?.FirstOrDefault(d => uid > 2000000000 ? d.Message.ChatId == uid - 2000000000 : d.User.Profile.Id == uid);
            if (dialog != null)
            {
                dialog.IsRead = (message.Flags & VkLongPollMessageFlags.Unread) == VkLongPollMessageFlags.Unread;
            }
        }

        private async void OnMessageFlagsAdded(LongPollMessageFlagsAdded message)
        {
            long uid = message.UserId;
            var dialog = Dialogs?.FirstOrDefault(d => uid > 2000000000 ? d.Message.ChatId == uid - 2000000000 : d.User.Profile.Id == uid);
            if (dialog != null)
            {
                dialog.IsRead = (message.Flags & VkLongPollMessageFlags.Unread) == VkLongPollMessageFlags.Unread;

                if ((message.Flags & VkLongPollMessageFlags.Deleted) == VkLongPollMessageFlags.Deleted)
                {
                    try
                    {
                        var response = await ServiceLocator.Vkontakte.Messages.GetHistory(uid > 2000000000 ? 0 : uid,
                                    uid > 2000000000 ? uid - 2000000000 : 0, count: 1);

                        if (response.TotalCount == 0)
                        {
                            Dialogs.Remove(dialog);
                        }
                        else
                        {
                            var title = dialog.Message.Title;
                            var newLastMessages = response.Items.First();
                            newLastMessages.Title = title;
                            dialog.Message = newLastMessages;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                }
            }
        }

        private void OnNewDialogStarted(NewDialogStartedMessage message)
        {
            var dialog = Dialogs?.FirstOrDefault(d => d.User?.Profile.Id == message.UserId);
            if (dialog != null)
                SelectedDialog = dialog;
        }

        private void OnGoToDialogMessage(GoToDialogMessage message)
        {
            long uid = message.UserId;

            var activeDialog = Dialogs.FirstOrDefault(d => uid > 2000000000 ? d.Message.ChatId == uid - 2000000000 : d.User?.Profile.Id == uid);
            SelectedDialog = activeDialog;

            Navigator.Content.Navigate(typeof(ChatView), new ConversationViewModel(uid));
        }

        private void OnLoginStateChanged(LoginStateChangedMessage message)
        {
            if (!message.IsLoggedIn)
            {
                UnsubscribeMessages();
            }
        }

        #endregion
    }
}