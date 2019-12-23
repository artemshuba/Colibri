using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Colibri.Helpers;
using Colibri.Model;
using Colibri.Services;
using Colibri.View;
using Colibri.ViewModel.Messaging;
using GalaSoft.MvvmLight.Messaging;
using Jupiter.Utils.Extensions;
using VkLib.Core.Attachments;
using VkLib.Core.Messages;
using VkLib.Core.Users;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Colibri.Controls
{
    public sealed partial class MessageControl : UserControl
    {
        private Size _lastSize;
        private CancellationTokenSource _forwardsCancellationTokenSource;

        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
            "Message", typeof(Message), typeof(MessageControl), new PropertyMetadata(default(Message), MessagePropertyChanged));

        private static void MessagePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (MessageControl)d;

            control.UpdateContent();
        }

        public Message Message
        {
            get { return (Message)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        public MessageControl()
        {
            this.InitializeComponent();
        }

        private void UpdateContent()
        {
            if (_forwardsCancellationTokenSource != null)
                _forwardsCancellationTokenSource.Cancel();
            _forwardsCancellationTokenSource = new CancellationTokenSource();

            AttachmentsPanel.Children.Clear();
            PhotosControl.Children.Clear();

            if (Message != null)
            {
                this.Foreground = (Brush)(Message.MessageContent.IsOut
                                        ? Application.Current.Resources["ConversationOutboxMessageForegroundBrush"]
                                        : Application.Current.Resources["ConversationInboxMessageForegroundBrush"]);

                BodyTextBlock.Visibility = !string.IsNullOrEmpty(Message.MessageContent.Body)
                    ? Visibility.Visible
                    : Visibility.Collapsed;

                TextBlockExtension.SetFormattedText(BodyTextBlock, Message.MessageContent.Body);

                if (Message.MessageContent.IsOut)
                    BodyTextBlock.SelectionHighlightColor = (SolidColorBrush)Application.Current.Resources["ChatMessageOutHighlightingTextBrush"];
                else
                    BodyTextBlock.SelectionHighlightColor = (SolidColorBrush)Application.Current.Resources["SystemControlHighlightAccentBrush"];

                bool isOut = Message.MessageContent.IsOut;
                TextBlockExtension.SetHyperlinkForeground(BodyTextBlock, (Brush)(isOut ? Application.Current.Resources["ConversationOutboxHyperlinkForegroundBrush"] :
                     Application.Current.Resources["ConversationInboxHyperlinkForegroundBrush"]));

                //BodyTextBlock.SelectionHighlightColor = (SolidColorBrush)BootStrapper.Current.Resources[isOut ? "ChatMessageOutboxHighlightingTextBrush" : "ChatMessageInboxHighlightingTextBrush"];

                if (!Message.MessageContent.Attachments.IsNullOrEmpty())
                {
                    AttachmentsPanel.Visibility = Visibility.Visible;

                    if (Message.MessageContent.Attachments.Any(a => a is VkPhotoAttachment))
                    {
                        PhotosControl.Visibility = Visibility.Visible;

                        foreach (var attachment in Message.MessageContent.Attachments.OfType<VkPhotoAttachment>())
                        {
                            var photo = (VkPhotoAttachment)attachment;

                            var button = new Button();
                            button.Style = (Style)Application.Current.Resources["SimpleButtonStyle"];

                            var image = new Image();
                            image.Stretch = Stretch.UniformToFill;
                            button.MaxWidth = photo.Width;
                            button.MaxHeight = photo.Height;
                            image.AddHandler(TappedEvent, new TappedEventHandler(OnImageTapped), false);
                            button.Content = image;

                            var sourceUrl = photo.SourceXBig ?? photo.Source ?? photo.SourceSmall;
                            if (sourceUrl != null)
                            {
                                image.Source = new BitmapImage(new Uri(sourceUrl));
                            }

                            PhotosControl.Children.Add(button);
                        }

                        if (PhotosControl.Children.Count > 0)
                        {
                            PhotoSpacier.Calculate(PhotosControl);
                        }
                    }
                    else
                    {
                        PhotosControl.Visibility = Visibility.Collapsed;
                    }

                    var attachments = Message.MessageContent.Attachments.Where(a => !(a is VkPhotoAttachment)).ToList();
                    var i = 0;
                    foreach (var attachment in attachments)
                    {
                        if (attachment is VkStickerAttachment)
                        {
                            var sticker = (VkStickerAttachment)attachment;

                            var image = new Image();
                            image.Stretch = Stretch.Uniform;
                            var thumbSize = CalcThumbnailSize(sticker.Width, sticker.Height, 200, 200);
                            image.Width = thumbSize.Width;
                            image.Height = thumbSize.Height;

                            var sourceUrl = sticker.Photo256;
                            if (sourceUrl != null)
                            {
                                image.Source = new BitmapImage(new Uri(sourceUrl));
                            }

                            AttachmentsPanel.Children.Add(image);
                        }
                        else if (attachment is VkGiftAttachment)
                        {
                            var gift = (VkGiftAttachment)attachment;

                            var image = new Image();
                            image.Stretch = Stretch.UniformToFill;
                            image.Width = 200;
                            image.Height = 200;

                            var sourceUrl = gift.Thumb256;
                            if (sourceUrl != null)
                            {
                                image.Source = new BitmapImage(new Uri(sourceUrl));
                            }

                            AttachmentsPanel.Children.Add(image);
                        }
                        else if (attachment is VkAudioAttachment)
                        {
                            var audio = (VkAudioAttachment)attachment;

                            var audioControl = new MessageAudioControl();
                            audioControl.Margin = new Thickness(0, 0, 0, i < attachments.Count - 1 ? 5 : 0);
                            audioControl.Audio = audio;
                            AttachmentsPanel.Children.Add(audioControl);
                        }
                        else if (attachment is VkLinkAttachment)
                        {
                            var link = (VkLinkAttachment)attachment;

                            var linkControl = new MessageLinkControl(link);
                            AttachmentsPanel.Children.Add(linkControl);
                        }
                        else if (attachment is VkDocumentAttachment)
                        {
                            var document = (VkDocumentAttachment)attachment;

                            var documentControl = new MessageDocumentControl(document);
                            documentControl.Margin = new Thickness(0, 0, 0, i < attachments.Count - 1 ? 5 : 0);
                            AttachmentsPanel.Children.Add(documentControl);
                        }
                        else if (attachment is VkVideoAttachment)
                        {
                            var video = (VkVideoAttachment)attachment;

                            var videoControl = new MessageVideoControl(video);
                            videoControl.Margin = new Thickness(0, 0, 0, i < attachments.Count - 1 ? 5 : 0);
                            AttachmentsPanel.Children.Add(videoControl);
                        }
                        else if (attachment is VkWallPostAttachment)
                        {
                            var wallPost = (VkWallPostAttachment)attachment;

                            var wallPostControl = new MessageWallPostControl(wallPost);
                            AttachmentsPanel.Children.Add(wallPostControl);
                        }

                        i++;
                    }
                }
                else if (Message.MessageContent.Geo != null)
                {
                    AttachmentsPanel.Visibility = Visibility.Visible;

                    var url = MapHelper.GetMapPreviewForCoords(Message.MessageContent.Geo.Latitude, Message.MessageContent.Geo.Longitude);

                    var button = new Button();
                    button.Style = (Style)Application.Current.Resources["SimpleButtonStyle"];

                    var image = new Image();
                    image.Stretch = Stretch.UniformToFill;
                    button.Width = 200;
                    button.Height = 200;
                    image.Tag = Message.MessageContent.Geo;
                    image.Source = new BitmapImage(new Uri(url));
                    button.Content = image;

                    image.AddHandler(TappedEvent, new TappedEventHandler(OnMapImageTapped), false);

                    AttachmentsPanel.Children.Add(button);
                }
                else if (!Message.MessageContent.ForwardMessages.IsNullOrEmpty())
                {
                    AttachmentsPanel.Visibility = Visibility.Visible;

                    foreach (var forwardMessage in Message.MessageContent.ForwardMessages)
                    {
                        var forwardMessageControl = new MessageForwardedMessageControl();
                        forwardMessageControl.Message = new Message(forwardMessage, new VkProfile());
                        forwardMessageControl.Foreground = this.Foreground;

                        AttachmentsPanel.Children.Add(forwardMessageControl);
                    }

                    LoadForwardsUsers();
                }
                else
                    AttachmentsPanel.Visibility = Visibility.Collapsed;

                if (Message.IsNew)
                {
                    Message.IsNew = false;

                    var s = (Storyboard)Resources["LoadAnim"];
                    s.Stop();
                    Storyboard.SetTarget(s, this.GetVisualAncestors().OfType<ListViewItem>().FirstOrDefault());
                    s.Begin();
                }
            }
        }

        private void OnImageTapped(object sender, TappedRoutedEventArgs tappedRoutedEventArgs)
        {
            var image = (Image)sender;
            var bi = (BitmapImage)image.Source;

            var p = new Dictionary<string, object>();
            p.Add("photos", Message.MessageContent.Attachments.OfType<VkPhotoAttachment>().Select(x => x.SourceMax).ToList());
            p.Add("currentPhoto", bi.UriSource.OriginalString);
            Navigator.NavigateAdaptive(typeof(PhotosPreviewView), p);
        }

        private void OnMapImageTapped(object sender, TappedRoutedEventArgs tappedRoutedEventArgs)
        {
            var image = (Image)sender;
            var geo = image.Tag as VkGeo;
            if (geo != null)
                MapHelper.OpenMap(geo.Latitude, geo.Longitude, geo.Place?.Title);
        }

        private Size CalcThumbnailSize(double originalWidth, double originalHeight, double maxWidth, double maxHeight)
        {
            if (originalWidth == 0 || originalHeight == 0)
                return new Size(maxWidth, maxHeight);

            var ratioX = (double)maxWidth / originalWidth;
            var ratioY = (double)maxHeight / originalHeight;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(originalWidth * ratio);
            var newHeight = (int)(originalHeight * ratio);

            return new Size(newWidth, newHeight);
        }

        private void MessageControl_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (Message?.MessageContent?.Attachments?.OfType<VkPhotoAttachment>().Count() > 0)
            {
                if (_lastSize != e.NewSize)
                {
                    PhotosControl.MaxWidth = Math.Max(180, e.NewSize.Width + 5);
                    if (PhotosControl.Children.Count > 0)
                    {
                        PhotoSpacier.Calculate(PhotosControl);
                    }
                }
            }

            _lastSize = e.NewSize;
        }

        private async void LoadForwardsUsers()
        {
            var ids = GetUserIdsRecursive(Message.MessageContent);
            ids = ids.Distinct().ToList();

            try
            {
                var token = _forwardsCancellationTokenSource.Token;
                var users = await ServiceLocator.UserService.GetById(ids);
                if (token.IsCancellationRequested)
                    return;

                if (users != null)
                {
                    foreach (var forwardedMessageControl in AttachmentsPanel.Children.OfType<MessageForwardedMessageControl>())
                    {
                        SetForwardedUsersRecursive(forwardedMessageControl, users);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to load forward messages users");
            }
        }

        private List<long> GetUserIdsRecursive(VkMessage message)
        {
            var result = new List<long>();
            if (message != Message.MessageContent)
                result.Add(message.UserId);
            if (!message.ForwardMessages.IsNullOrEmpty())
            {
                foreach (var forwardMessage in message.ForwardMessages)
                {
                    result.AddRange(GetUserIdsRecursive(forwardMessage));
                }
            }

            return result;
        }

        private void SetForwardedUsersRecursive(MessageForwardedMessageControl control, List<VkProfile> users)
        {
            control.User = users.FirstOrDefault(u => u.Id == control.Message.MessageContent.UserId);

            foreach (var child in control.AttachmentsPanel.Children.OfType<MessageForwardedMessageControl>())
            {
                SetForwardedUsersRecursive(child, users);
            }
        }

        private void ContextMenuForwardClick(object sender, RoutedEventArgs e)
        {
            ForwardMessagesHelper.PendingForwardMessage = Message;
            Navigator.Content.Navigate(typeof(NewDialogView));
        }

        private async void ContextMenuDeleteClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (await ServiceLocator.Vkontakte.Messages.Delete(new List<long>() { Message.MessageContent.Id }))
                {
                    Messenger.Default.Send(new LongPollMessageDeleted() { MessageId = Message.MessageContent.Id });
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to delete message");
            }
        }

        private async void ContextMenuMarkAsReadClick(object sender, RoutedEventArgs e)
        {
            try
            {
                await ServiceLocator.Vkontakte.Messages.MarkAsRead(new List<long>() { Message.MessageContent.Id });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to mark message as read");
            }
        }
    }
}