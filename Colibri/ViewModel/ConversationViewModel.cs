using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.PushNotifications;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Colibri.Helpers;
using Colibri.Model;
using Colibri.Services;
using Colibri.View;
using Colibri.ViewModel.Messaging;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Jupiter.Mvvm;
using Jupiter.Services.Navigation;
using Jupiter.Utils.Extensions;
using Jupiter.Utils.Helpers;
using VkLib.Core.Attachments;
using VkLib.Core.Messages;
using VkLib.Core.Store;
using VkLib.Core.Users;
using VkLib.Error;
using User = Colibri.Model.User;
using Windows.UI.Xaml;
using Jupiter.Application;
using System.Globalization;

namespace Colibri.ViewModel
{
    public class ConversationViewModel : ViewModelBase
    {
        private Dialog _dialog;
        private IncrementalTopLoadingCollection<Message> _messages;
        private int _totalCount;
        private bool _hasUnreadMessages = false;

        private string _messageToSend;

        private DateTime _lastTypingNotificationTime;

        private bool _isTyping;
        private CancellationTokenSource _userTypingCancellationToken;

        //последние отправленные сообщения, для синхронизации с LongPoll
        private List<Message> _lastSentMessages = new List<Message>();

        private ObservableCollection<AttachmentUpload> _attachmentUploads = new IncrementalTopLoadingCollection<AttachmentUpload>();

        private ManualResetEventSlim _uploadsEvent = new ManualResetEventSlim(true);

        #region Commands

        public RelayCommand SendMessageCommand { get; private set; }

        public RelayCommand<VkStickerProduct> SendStickerCommand { get; private set; }

        public RelayCommand UserReadMessageCommand { get; private set; }

        public RelayCommand GoToUserProfileCommand { get; private set; }



        public RelayCommand AttachPhotoCommand { get; private set; }

        public RelayCommand AttachDocumentCommand { get; private set; }

        public RelayCommand AttachVideoCommand { get; private set; }

        public RelayCommand AttachLocationCommand { get; private set; }

        public RelayCommand<AttachmentUpload> RemoveAttachmentCommand { get; private set; }

        #endregion

        public Dialog Dialog
        {
            get { return _dialog; }
            protected set
            {
                if (Set(ref _dialog, value))
                {
                    RaisePropertyChanged("UserOnlineStatus");
                    RaisePropertyChanged("IsChat");
                    RaisePropertyChanged("Title");
                }
            }
        }

        public IncrementalTopLoadingCollection<Message> Messages
        {
            get { return _messages; }
            protected set { Set(ref _messages, value); }
        }

        public string MessageToSend
        {
            get { return _messageToSend; }
            set
            {
                if (Set(ref _messageToSend, value))
                    NotifyTyping();
            }
        }

        public string UserOnlineStatus
        {
            get
            {
                if (_isTyping)
                    return Localizator.String("UserStatusTyping");

                if (Dialog != null && Dialog.Message != null && Dialog.Message.ChatId != 0)
                {
                    var count = Dialog.Message?.UsersCount ?? 0;
                    var locale = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

                    return count + " " +
                           StringHelper.LocalizeNumerals(count, Localizator.String("ConversationPersonSingular"),
                               Localizator.String("ConversationPersonDual"),
                               Localizator.String("ConversationPersonPlural"), locale);
                }

                if (Dialog?.User == null)
                    return null;

                if (Dialog.User.IsOnline)
                    return Localizator.String("UserStatusOnline");

                return DateFormatHelper.FormatUserLastSeen(Dialog.User.Profile);
            }
        }

        public string Title
        {
            get
            {
                if (Dialog != null)
                {
                    if (Dialog.Message?.ChatId != 0)
                        return Dialog.Message?.Title.ToUpper();
                    else
                        return Dialog.User?.Profile.Name.ToUpper();
                }

                return null;
            }
        }

        public bool IsChat => Dialog?.Message.ChatId != 0;

        public ObservableCollection<AttachmentUpload> AttachmentUploads
        {
            get { return _attachmentUploads; }
        }

        public ConversationViewModel()
        {
            _messages = new IncrementalTopLoadingCollection<Message>();
            Messages.OnMoreItemsRequested = LoadMoreMessages;
            Messages.HasMoreItemsRequested = () => _totalCount > Messages.Count;

            RegisterTasks("history");

            SubscribeMessages();
        }

        public ConversationViewModel(Dialog dialog) : this()
        {
            _dialog = dialog;

#if !MOCK
            bool isChat = Dialog.Message.ChatId != 0;
            CheckForwardMessages(isChat ? Dialog.Message.ChatId : Dialog.User.Profile.Id);

            Load();
#endif
        }

        public ConversationViewModel(long userId) : this()
        {
            if (userId > 2000000000)
            {
                var chatId = userId - 2000000000;
                LoadChat(chatId);
                CheckForwardMessages(chatId);
            }
            else
            {
                LoadUser(userId);
                CheckForwardMessages(userId);
            }
        }

        ~ConversationViewModel()
        {
            UnsubscribeMessages();
        }

        protected override void InitializeCommands()
        {
            SendMessageCommand = new RelayCommand(SendMessage);

            SendStickerCommand = new RelayCommand<VkStickerProduct>(SendSticker);

            UserReadMessageCommand = new RelayCommand(UserReadMessage);

            AttachPhotoCommand = new RelayCommand(async () =>
            {
                var filePicker = new FileOpenPicker();
                filePicker.ViewMode = PickerViewMode.Thumbnail;
                filePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                filePicker.FileTypeFilter.Add(".jpg");
                filePicker.FileTypeFilter.Add(".jpeg");
                filePicker.FileTypeFilter.Add(".png");
                StorageFile file = await filePicker.PickSingleFileAsync();
                if (file != null)
                {
                    AttachPhotoFromFile(file);
                }
            });

            AttachDocumentCommand = new RelayCommand(async () =>
            {
                var filePicker = new FileOpenPicker();
                filePicker.ViewMode = PickerViewMode.List;
                filePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                filePicker.FileTypeFilter.Add("*");

                StorageFile file = await filePicker.PickSingleFileAsync();
                if (file != null)
                {
                    AttachDocumentFromFile(file);
                }
            });

            AttachVideoCommand = new RelayCommand(async () =>
            {
                var filePicker = new FileOpenPicker();
                filePicker.ViewMode = PickerViewMode.List;
                filePicker.SuggestedStartLocation = PickerLocationId.VideosLibrary;
                filePicker.FileTypeFilter.Add(".avi");
                filePicker.FileTypeFilter.Add(".mp4");
                filePicker.FileTypeFilter.Add(".3gp");
                filePicker.FileTypeFilter.Add(".mpeg");
                filePicker.FileTypeFilter.Add(".mov");
                filePicker.FileTypeFilter.Add(".mp3");
                filePicker.FileTypeFilter.Add(".flv");
                filePicker.FileTypeFilter.Add(".wmv");

                StorageFile file = await filePicker.PickSingleFileAsync();
                if (file != null)
                {
                    AttachVideoFromFile(file);
                }
            });

            AttachLocationCommand = new RelayCommand(() =>
            {
                //TODO
            });

            RemoveAttachmentCommand = new RelayCommand<AttachmentUpload>(attachment =>
            {
                bool notify = AttachmentUploads.Count == 1;

                if (attachment is ForwardMessagesAttachmentUpload)
                {
                    bool isChat = Dialog.Message.ChatId != 0;
                    long dialogId = isChat ? Dialog.Message.ChatId : Dialog.User.Profile.Id;

                    if (ForwardMessagesHelper.PendingForwardMessages.ContainsKey(dialogId))
                    {
                        ForwardMessagesHelper.PendingForwardMessages[dialogId].Clear();
                        ForwardMessagesHelper.PendingForwardMessages[dialogId] = null;
                        ForwardMessagesHelper.PendingForwardMessages.Remove(dialogId);
                    }
                }

                AttachmentUploads.Remove(attachment);

                if (notify)
                    RaisePropertyChanged("AttachmentUploads");
            });

            GoToUserProfileCommand = new RelayCommand(async () =>
            {
                //TODO пока открываем страницу в браузере
                await Launcher.LaunchUriAsync(new Uri("http://vk.com/id" + Dialog.User.Profile.Id));
            });
        }

        public override void OnNavigatingFrom(NavigatingEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back || (e.TargetPageType == e.PageType))
                UnsubscribeMessages();

            base.OnNavigatingFrom(e);
        }

        public async void AttachPhotoFromFile(StorageFile file)
        {
            bool notify = AttachmentUploads.Count == 0;

            // Application now has read/write access to the picked file
            var photoAttachment = new PhotoAttachmentUpload();
            var fileStream = await file.OpenReadAsync();

            var stream = fileStream.CloneStream();

            fileStream.Dispose();

            photoAttachment.Stream = stream.AsStreamForRead();
            photoAttachment.FileName = file.Name;

            var bi = new BitmapImage();
            await bi.SetSourceAsync(stream);
            photoAttachment.Photo = bi;

            AttachmentUploads.Add(photoAttachment);

            UploadPhotoAttachment(photoAttachment);

            if (notify)
                RaisePropertyChanged("AttachmentUploads");
        }

        public async void AttachPhotoFromStream(IRandomAccessStream imageStream, string contentType)
        {
            bool notify = AttachmentUploads.Count == 0;

            // Application now has read/write access to the picked file
            var photoAttachment = new PhotoAttachmentUpload();

            photoAttachment.Stream = imageStream.AsStreamForRead();
            photoAttachment.FileName = "Image." + contentType.Substring(contentType.IndexOf("/") + 1);

            var bi = new BitmapImage();
            await bi.SetSourceAsync(imageStream);
            photoAttachment.Photo = bi;

            AttachmentUploads.Add(photoAttachment);

            UploadPhotoAttachment(photoAttachment);

            if (notify)
                RaisePropertyChanged("AttachmentUploads");
        }

        public async void AttachDocumentFromFile(StorageFile file)
        {
            bool notify = AttachmentUploads.Count == 0;

            var docAttachment = new DocumentAttachmentUpload();
            var fileStream = await file.OpenReadAsync();

            var stream = fileStream.CloneStream();

            fileStream.Dispose();

            docAttachment.Stream = stream.AsStreamForRead();
            docAttachment.FileName = file.Name;

            AttachmentUploads.Add(docAttachment);

            UploadDocumentAttachment(docAttachment);

            if (notify)
                RaisePropertyChanged("AttachmentUploads");
        }

        public async void AttachVideoFromFile(StorageFile file)
        {
            bool notify = AttachmentUploads.Count == 0;

            var videoAttachment = new VideoAttachmentUpload();
            var fileStream = await file.OpenReadAsync();

            var stream = fileStream.CloneStream();

            fileStream.Dispose();

            videoAttachment.Stream = stream.AsStreamForRead();
            videoAttachment.FileName = file.Name;

            AttachmentUploads.Add(videoAttachment);

            UploadVideoAttachment(videoAttachment);

            if (notify)
                RaisePropertyChanged("AttachmentUploads");
        }

        public void AttachMessage(Message message)
        {
            if (AttachmentUploads.OfType<ForwardMessagesAttachmentUpload>().Any())
            {
                var messagesAttachment = AttachmentUploads.OfType<ForwardMessagesAttachmentUpload>().First();
                messagesAttachment.Messages.Add(message);
            }
            else
            {
                bool notify = AttachmentUploads.Count == 0;

                var messagesAttachment = new ForwardMessagesAttachmentUpload();
                messagesAttachment.Messages.Add(message);
                AttachmentUploads.Add(messagesAttachment);

                if (notify)
                    RaisePropertyChanged("AttachmentUploads");
            }
        }

        private void CheckForwardMessages(long dialogId)
        {
            if (ForwardMessagesHelper.PendingForwardMessages.ContainsKey(dialogId))
            {
                var messages = ForwardMessagesHelper.PendingForwardMessages[dialogId];
                foreach (var message in messages)
                {
                    AttachMessage(message);
                }
            }

            if (ForwardMessagesHelper.PendingForwardMessage != null)
            {
                AttachMessage(ForwardMessagesHelper.PendingForwardMessage);

                if (!ForwardMessagesHelper.PendingForwardMessages.ContainsKey(dialogId))
                    ForwardMessagesHelper.PendingForwardMessages[dialogId] = new List<Message>();

                ForwardMessagesHelper.PendingForwardMessages[dialogId].Add(ForwardMessagesHelper.PendingForwardMessage);
                ForwardMessagesHelper.PendingForwardMessage = null;
            }
        }

        private async void Load()
        {
            var t = TaskStarted("history");

            try
            {
                bool isChat = Dialog.Message.ChatId != 0;
                if (isChat)
                {
                    var users = await ServiceLocator.UserService.GetChatUsers(Dialog.Message.ChatId);

                    Dialog.Users = users;

                    if (Dialog.Users == null)
                        Dialog.Users = new List<VkProfile>();

                    if (!Dialog.Users.Contains(ViewModelLocator.Main.CurrentUser))
                        Dialog.Users.Add(ViewModelLocator.Main.CurrentUser);
                }

                var vkMessages = await ServiceLocator.Vkontakte.Execute.GetChatHistoryAndMarkAsRead(!isChat ? Dialog.User.Profile.Id : 0, chatId: isChat ? Dialog.Message.ChatId : 0, count: DeviceHelper.IsMobile() ? 20 : 50, markAsRead: !AppSettings.DontMarkMessagesAsRead);
                if (!vkMessages.Items.IsNullOrEmpty())
                {
                    if (isChat)
                    {
                        var userIds = vkMessages.Items.Where(m => !Dialog.Users.Any(u => m.UserId == u.Id)).Select(m => m.UserId).ToList();

                        var users = await ServiceLocator.UserService.GetById(userIds);

                        if (!users.IsNullOrEmpty())
                        {
                            Dialog.Users.AddRange(users);
                        }
                    }

                    Messages.AddRange(vkMessages.Items.Select(m =>
                    {
                        VkProfile sender = null;
                        if (!isChat)
                            sender = !m.IsOut ? Dialog.User.Profile : null;
                        else
                            sender = Dialog.Users?.FirstOrDefault(u => u.Id == m.UserId);
                        return new Message(m, sender);
                    }).Reverse().ToList());
                    _totalCount = vkMessages.TotalCount;
                    //Messages.OnMoreItemsRequested = LoadMoreMessages;
                    //Messages.HasMoreItemsRequested = () => _totalCount > Messages.Count;
                }
            }
            catch (VkInvalidTokenException)
            {
                Messenger.Default.Send(new LoginStateChangedMessage() { IsLoggedIn = false });

                Navigator.Main.Navigate(typeof(LoginView), clearHistory: true);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to load messages in coversation");

                t.Error = Localizator.String("Errors/ChatHistoryCommonError");
            }

            t.IsWorking = false;
        }

        private async Task<List<Message>> LoadMoreMessages(CancellationToken token, uint count)
        {
            try
            {
                bool isChat = Dialog.Message.ChatId != 0;
                var vkMessages = await ServiceLocator.Vkontakte.Execute.GetChatHistoryAndMarkAsRead(!isChat ? Dialog.User.Profile.Id : 0, chatId: isChat ? Dialog.Message.ChatId : 0, count: (int)count, offset: Messages.Count, markAsRead: !AppSettings.DontMarkMessagesAsRead);
                if (!vkMessages.Items.IsNullOrEmpty())
                {
                    TaskFinished("history"); //таска может быть активна, тогда LoadingIndicator будет накладываться на список
                    _hasUnreadMessages = true;

                    return vkMessages.Items.Select(m =>
                    {
                        VkProfile sender = null;

                        if (!isChat)
                            sender = !m.IsOut ? Dialog.User.Profile : null;
                        else
                            sender = Dialog.Users?.FirstOrDefault(u => u.Id == m.UserId);
                        return new Message(m, sender);
                    }).ToList();
                }
            }
            catch (VkInvalidTokenException)
            {
                Messenger.Default.Send(new LoginStateChangedMessage() { IsLoggedIn = false });

                Navigator.Main.Navigate(typeof(LoginView), clearHistory: true);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to load more messages in coversation");
            }

            return null;
        }

        private async void LoadUser(long userId)
        {
            try
            {
                var user = await ServiceLocator.UserService.GetById(userId);
                if (user != null)
                {
                    var dialog = new Dialog();
                    dialog.User = new User(user);
                    dialog.Message = new VkMessage();
                    Dialog = dialog;

                    Load();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to load conversation user with id " + userId);
            }
        }

        private async void LoadChat(long chatId)
        {
            try
            {
                var chat = await ServiceLocator.UserService.GetChat(chatId);
                if (chat != null)
                {
                    var dialog = new Dialog();
                    dialog.Users = chat.Users;
                    dialog.Message = new VkMessage() { ChatId = chatId, Title = chat.Title };
                    Dialog = dialog;

                    Load();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to load chat with id " + chatId);
            }
        }

        private async void SendMessage()
        {
            if (string.IsNullOrWhiteSpace(MessageToSend) && AttachmentUploads.Count == 0)
                return;

            var message = MessageToSend?.Trim();
            MessageToSend = null;

            var newMessage = new Message(new VkMessage() { Body = message, IsOut = true, Date = DateTime.Now },
                ViewModelLocator.Main.CurrentUser);
            newMessage.IsNew = true;

            try
            {
                newMessage.IsSent = false;

                bool hasPendingUploads = AttachmentUploads.Any(a => a.IsUploading);
                bool isChat = Dialog.Message.ChatId != 0;

                if (AttachmentUploads.Count > 0)
                {
                    if (hasPendingUploads)
                        await Task.Run(() => _uploadsEvent.Wait()); //ждем завершение загрузок

                    newMessage.MessageContent.Attachments = new List<VkAttachment>();

                    foreach (var attachment in AttachmentUploads)
                    {
                        if (attachment is PhotoAttachmentUpload)
                        {
                            var photo = (PhotoAttachmentUpload)attachment;

                            newMessage.MessageContent.Attachments.Add(new VkPhotoAttachment(photo.VkPhoto));
                        }
                        else if (attachment is DocumentAttachmentUpload)
                        {
                            var document = (DocumentAttachmentUpload)attachment;

                            newMessage.MessageContent.Attachments.Add(document.VkDocument);
                        }
                        else if (attachment is VideoAttachmentUpload)
                        {
                            var video = (VideoAttachmentUpload)attachment;

                            newMessage.MessageContent.Attachments.Add(new VkVideoAttachment(video.VkVideo));
                        }
                        else if (attachment is ForwardMessagesAttachmentUpload)
                        {
                            var forwardMessages = (ForwardMessagesAttachmentUpload)attachment;

                            newMessage.MessageContent.ForwardMessages = forwardMessages.Messages.Select(m => m.MessageContent).ToList();

                            long dialogId = isChat ? Dialog.Message.ChatId : Dialog.User.Profile.Id;
                            ForwardMessagesHelper.PendingForwardMessages[dialogId].Clear();
                            ForwardMessagesHelper.PendingForwardMessages[dialogId] = null;
                            ForwardMessagesHelper.PendingForwardMessages.Remove(dialogId);
                        }
                    }

                    AttachmentUploads.Clear();
                }

                Messages.Add(newMessage);
                _lastSentMessages.Add(newMessage);

                var newMessageId = await ServiceLocator.Vkontakte.Messages.Send(!isChat ? Dialog.User.Profile.Id : 0, isChat ? Dialog.Message.ChatId : 0, message, newMessage.MessageContent.Attachments, newMessage.MessageContent.ForwardMessages);
                newMessage.MessageContent.Id = newMessageId;
                newMessage.IsSent = true;
                _lastSentMessages.Remove(newMessage);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to send message");

                newMessage.IsFailed = true;
            }
        }

        private async void SendSticker(VkStickerProduct sticker)
        {
            var newMessage = new Message(new VkMessage() { IsOut = true, Date = DateTime.Now }, ViewModelLocator.Main.CurrentUser);
            newMessage.IsNew = true;

            try
            {
                newMessage.IsSent = false;
                newMessage.MessageContent.Attachments = new List<VkAttachment>()
                {
                    new VkStickerAttachment()
                    {
                        ProductId = sticker.Id,
                        Width = 200,
                        Height = 200,
                        Photo256 = sticker.GetPreviewUrl(256)
                    }
                };

                bool isChat = Dialog.Message.ChatId != 0;

                Messages.Add(newMessage);
                _lastSentMessages.Add(newMessage);

                var newMessageId = await ServiceLocator.Vkontakte.Messages.Send(!isChat ? Dialog.User.Profile.Id : 0, isChat ? Dialog.Message.ChatId : 0, null, stickerId: sticker.Id);
                newMessage.MessageContent.Id = newMessageId;
                newMessage.IsSent = true;
                _lastSentMessages.Remove(newMessage);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to send sticker message");

                newMessage.IsFailed = true;
            }
        }

        //Marks messages as read. Called if current user shows he is here (moved pointer, scrolled chat)
        private async void UserReadMessage()
        {
            if (AppSettings.DontMarkMessagesAsRead)
                return;

            if (!_hasUnreadMessages || Dialog?.User == null)
                return;


            _hasUnreadMessages = false;

            try
            {
                bool isChat = Dialog.Message.ChatId != 0;
                await ServiceLocator.Vkontakte.Messages.MarkAsRead(null, peerId: !isChat ? Dialog.User.Profile.Id.ToString() : (2000000000 + Dialog.Message.ChatId).ToString());
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to mark messages as read");
            }
        }

        private async void NotifyTyping()
        {
            if ((DateTime.Now - _lastTypingNotificationTime).TotalSeconds < 10)
                return;

            _lastTypingNotificationTime = DateTime.Now;

            if (AppSettings.HideOnlineStatus)
                return;

            try
            {
                bool isChat = Dialog.Message.ChatId != 0;
                await ServiceLocator.Vkontakte.Messages.SetActivity(peerId: !isChat ? Dialog.User.Profile.Id.ToString() : (2000000000 + Dialog.Message.ChatId).ToString());
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to send typing");
            }

        }

        private async void UploadPhotoAttachment(PhotoAttachmentUpload attachment)
        {
            attachment.IsUploading = true;

            _uploadsEvent.Reset();

            try
            {
                var uploadServer = await ServiceLocator.Vkontakte.Photos.GetMessagesUploadServer();
                attachment.Stream.Seek(0, SeekOrigin.Begin);

                if (attachment.CancellationToken.IsCancellationRequested)
                    return;

                var uploadResponse = await ServiceLocator.Vkontakte.Photos.UploadPhoto(uploadServer, attachment.FileName, attachment.Stream);

                if (attachment.CancellationToken.IsCancellationRequested)
                    return;

                if (uploadResponse != null)
                {
                    var photo = await ServiceLocator.Vkontakte.Photos.SaveMessagePhoto(uploadResponse.Server, uploadResponse.Photo, uploadResponse.Hash);

                    if (attachment.CancellationToken.IsCancellationRequested)
                        return;

                    if (photo != null)
                    {
                        attachment.VkPhoto = photo;
                        attachment.IsUploaded = true;
                    }
                }
            }
            catch (VkAccessDeniedException)
            {
                Messenger.Default.Send(new LoginStateChangedMessage() { IsLoggedIn = false });

                Navigator.Main.Navigate(typeof(LoginView), clearHistory: true);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to upload photo attachment");
            }
            finally
            {
                attachment.IsUploading = false;

                UpdatePendingAttachments();
            }
        }

        private async void UploadDocumentAttachment(DocumentAttachmentUpload attachment)
        {
            attachment.IsUploading = true;

            _uploadsEvent.Reset();

            try
            {
                var uploadServer = await ServiceLocator.Vkontakte.Documents.GetUploadServer();
                attachment.Stream.Seek(0, SeekOrigin.Begin);

                if (attachment.CancellationToken.IsCancellationRequested)
                    return;

                var uploadResponse = await ServiceLocator.Vkontakte.Documents.Upload(uploadServer, attachment.FileName, attachment.Stream);

                if (attachment.CancellationToken.IsCancellationRequested)
                    return;

                if (uploadResponse != null)
                {
                    var document = await ServiceLocator.Vkontakte.Documents.Save(uploadResponse.File);

                    if (attachment.CancellationToken.IsCancellationRequested)
                        return;

                    if (document != null)
                    {
                        attachment.VkDocument = document;
                        attachment.IsUploaded = true;
                    }
                }
            }
            catch (VkAccessDeniedException)
            {
                Messenger.Default.Send(new LoginStateChangedMessage() { IsLoggedIn = false });

                Navigator.Main.Navigate(typeof(LoginView), clearHistory: true);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to upload document attachment");
            }
            finally
            {
                attachment.IsUploading = false;

                UpdatePendingAttachments();
            }
        }

        private async void UploadVideoAttachment(VideoAttachmentUpload attachment)
        {
            attachment.IsUploading = true;

            _uploadsEvent.Reset();

            try
            {
                var uploadInfos = await ServiceLocator.Vkontakte.Video.Save(isPrivate: true);
                attachment.Stream.Seek(0, SeekOrigin.Begin);

                if (attachment.CancellationToken.IsCancellationRequested)
                    return;

                var uploadResponse = await ServiceLocator.Vkontakte.Video.Upload(uploadInfos.UploadUrl, attachment.FileName, attachment.Stream);

                if (attachment.CancellationToken.IsCancellationRequested)
                    return;

                if (uploadResponse != null)
                {
                    var videos = await ServiceLocator.Vkontakte.Video.Get(new List<string>() { ViewModelLocator.Main.CurrentUser?.Id + "_" + uploadResponse.VideoId });

                    if (attachment.CancellationToken.IsCancellationRequested)
                        return;

                    if (videos != null && !videos.Items.IsNullOrEmpty())
                    {
                        attachment.VkVideo = videos.Items.First();
                    }

                    attachment.IsUploaded = true;
                }
            }
            catch (VkAccessDeniedException)
            {
                Messenger.Default.Send(new LoginStateChangedMessage() { IsLoggedIn = false });

                Navigator.Main.Navigate(typeof(LoginView), clearHistory: true);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to upload video attachment");
            }
            finally
            {
                attachment.IsUploading = false;

                UpdatePendingAttachments();
            }
        }

        private void UpdatePendingAttachments()
        {
            if (!AttachmentUploads.Any(a => a.IsUploading))
                _uploadsEvent.Set();
        }

        #region Message interception

        private void SubscribeMessages()
        {
            Messenger.Default.Register<LongPollChatMessage>(this, OnChatMessage);
            Messenger.Default.Register<LongPollFriendOnlineStatusChangedMessage>(this, OnFriendOnlineStatusChanged);
            Messenger.Default.Register<LongPollUserStartedTypingMessage>(this, OnUserStartedTyping);
            Messenger.Default.Register<LongPollMessageFlagsRemoved>(this, OnMessageFlagsRemoved);
            Messenger.Default.Register<LongPollMessageFlagsAdded>(this, OnMessageFlagsAdded);
            Messenger.Default.Register<LongPollMessageDeleted>(this, OnMessageDeleted);
            Messenger.Default.Register<LoginStateChangedMessage>(this, OnLoginStateChanged);
            Messenger.Default.Register<PushNotificationReceivedMessage>(this, OnPushNotificationReceived);
        }

        private void UnsubscribeMessages()
        {
            Messenger.Default.Unregister<LongPollChatMessage>(this, OnChatMessage);
            Messenger.Default.Unregister<LongPollFriendOnlineStatusChangedMessage>(this, OnFriendOnlineStatusChanged);
            Messenger.Default.Unregister<LongPollUserStartedTypingMessage>(this, OnUserStartedTyping);
            Messenger.Default.Unregister<LongPollMessageFlagsRemoved>(this, OnMessageFlagsRemoved);
            Messenger.Default.Unregister<LongPollMessageFlagsAdded>(this, OnMessageFlagsAdded);
            Messenger.Default.Unregister<LongPollMessageDeleted>(this, OnMessageDeleted);
            Messenger.Default.Unregister<LoginStateChangedMessage>(this, OnLoginStateChanged);
            Messenger.Default.Unregister<PushNotificationReceivedMessage>(this, OnPushNotificationReceived);
        }

        private async void OnChatMessage(LongPollChatMessage message)
        {
            if (Messages == null)
                return;

            if (message.Message?.MessageContent == null)
                return;

            bool isChat = Dialog?.Message.ChatId != 0;
            if ((!isChat && (message.Message.MessageContent.UserId == Dialog?.User.Profile.Id)) ||
                (isChat && Dialog?.Message.ChatId == message.Message.MessageContent.ChatId))
            {
                var id = message.Message.MessageContent.Id;
                if (message.Message.MessageContent.IsOut)
                {
                    var existedMessage = Messages.FirstOrDefault(m => m.MessageContent.Id == id);
                    if (existedMessage != null)
                        return;
                    else if (_lastSentMessages.Count > 0)
                        return;
                }

                _hasUnreadMessages = true;

                message.Message.IsNew = true;
                Messages.Add(message.Message);

                //if there are attachments in message, request message from server with full data
                if (!message.Message.MessageContent.Attachments.IsNullOrEmpty() || message.Message.MessageContent.Geo != null || message.Message.MessageContent.ForwardMessages != null)
                {
                    try
                    {
                        var fullMessage = await ServiceLocator.Vkontakte.Messages.GetById(id);
                        if (fullMessage != null)
                        {
                            var m = Messages.FirstOrDefault(x => x.MessageContent.Id == id);
                            m.MessageContent = fullMessage;
                            Messages[Messages.IndexOf(m)] = m;
                            Debug.WriteLine("Updated");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "Unable to load full message by id " + id);
                    }
                }
            }
        }

        private void OnFriendOnlineStatusChanged(LongPollFriendOnlineStatusChangedMessage message)
        {
            if (Dialog?.User != null && Dialog?.User.Profile.Id == message.UserId)
            {
                Dialog.User.IsOnline = message.IsOnline;
                RaisePropertyChanged("UserOnlineStatus");
            }
        }

        private async void OnUserStartedTyping(LongPollUserStartedTypingMessage message)
        {
            if (Dialog?.User != null && Dialog?.User.Profile.Id == message.UserId)
            {
                _userTypingCancellationToken?.Cancel();
                _userTypingCancellationToken = new CancellationTokenSource();

                var token = _userTypingCancellationToken.Token;

                _isTyping = true;
                RaisePropertyChanged("UserOnlineStatus");

                try
                {
                    await Task.Delay(10000, token); //wait 10 secs
                }
                catch (TaskCanceledException) { }

                if (!token.IsCancellationRequested)
                {
                    _isTyping = false;
                    RaisePropertyChanged("UserOnlineStatus");
                }
            }
        }


        private void OnMessageFlagsRemoved(LongPollMessageFlagsRemoved message)
        {
            var m = Messages?.FirstOrDefault(x => x.MessageContent.Id == message.MessageId);
            if (m != null)
                m.IsRead = (message.Flags & VkLongPollMessageFlags.Unread) == VkLongPollMessageFlags.Unread;
        }

        private void OnMessageFlagsAdded(LongPollMessageFlagsAdded message)
        {
            var m = Messages?.FirstOrDefault(x => x.MessageContent.Id == message.MessageId);
            if (m != null)
            {
                m.IsRead = (message.Flags & VkLongPollMessageFlags.Unread) == VkLongPollMessageFlags.Unread;
                if ((message.Flags & VkLongPollMessageFlags.Deleted) == VkLongPollMessageFlags.Deleted)
                    Messages.Remove(m);
            }
        }

        private void OnMessageDeleted(LongPollMessageDeleted message)
        {
            var m = Messages?.FirstOrDefault(x => x.MessageContent.Id == message.MessageId);
            if (m != null)
                Messages.Remove(m);
        }

        private void OnLoginStateChanged(LoginStateChangedMessage message)
        {
            if (!message.IsLoggedIn)
            {
                Messages?.Clear();

                UnsubscribeMessages();
            }
        }

        private void OnPushNotificationReceived(PushNotificationReceivedMessage message)
        {
            var args = message.Args;
            if (args.NotificationType == PushNotificationType.Toast && args.ToastNotification != null)
            {
                Debug.WriteLine(args.ToastNotification.Content.GetXml());

                if (JupiterApp.Current.IsMinimized)
                    return;

                try
                {
                    var toasts = args.ToastNotification.Content.GetElementsByTagName("toast");
                    if (toasts.Length > 0)
                    {
                        var toast = toasts.Item(0);
                        var launchArgs = toast?.Attributes.GetNamedItem("launch");
                        if (launchArgs != null)
                        {
                            var p = ((string)launchArgs.NodeValue).ParseQueryString();
                            var uid = long.Parse(p["uid"]);
                            if (uid > 2000000000 && Dialog?.Message.ChatId == uid - 2000000000)
                                args.Cancel = true;
                            else if (uid == Dialog?.User?.Profile.Id)
                                args.Cancel = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Unable to process push notification");
                }
            }
        }

        #endregion
    }
}