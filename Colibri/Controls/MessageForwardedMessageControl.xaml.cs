using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Colibri.Helpers;
using Colibri.Model;
using Colibri.View;
using Jupiter.Utils.Extensions;
using VkLib.Core.Attachments;
using VkLib.Core.Messages;
using VkLib.Core.Users;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Colibri.Controls
{
    public sealed partial class MessageForwardedMessageControl : UserControl
    {
        private Size _lastSize;

        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
            "Message", typeof(Message), typeof(MessageForwardedMessageControl), new PropertyMetadata(default(Message), MessagePropertyChanged));

        private static void MessagePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (MessageForwardedMessageControl)d;

            control.UpdateContent();
        }

        public Message Message
        {
            get { return (Message)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        public static readonly DependencyProperty UserProperty = DependencyProperty.Register(
            "User", typeof(VkProfile), typeof(MessageForwardedMessageControl), new PropertyMetadata(default(VkProfile), OnUserPropertyChanged));

        private static void OnUserPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (MessageForwardedMessageControl)d;
            var user = e.NewValue as VkProfile;
            if (user != null)
            {
                control.AvatarControl.Avatar = new BitmapImage(new Uri(user.Photo));
                control.UserNameTextBlock.Text = user.Name;
            }
        }

        public VkProfile User
        {
            get { return (VkProfile)GetValue(UserProperty); }
            set { SetValue(UserProperty, value); }
        }

        public MessageForwardedMessageControl()
        {
            this.InitializeComponent();
        }

        private void UpdateContent()
        {
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

                bool isOut = Message.MessageContent.IsOut;
                TextBlockExtension.SetHyperlinkForeground(BodyTextBlock, (Brush)(isOut ? Application.Current.Resources["ConversationOutboxHyperlinkForegroundBrush"] :
                     Application.Current.Resources["ConversationInboxHyperlinkForegroundBrush"]));

                if (Message.MessageContent.IsOut)
                    BodyTextBlock.SelectionHighlightColor = (SolidColorBrush)Application.Current.Resources["ChatMessageOutHighlightingTextBrush"];
                else
                    BodyTextBlock.SelectionHighlightColor = (SolidColorBrush)Application.Current.Resources["SystemControlHighlightAccentBrush"];

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
                        forwardMessageControl.Margin = new Thickness(5, 0, 0, 0);
                        forwardMessageControl.SetBinding(ForegroundProperty, new Binding() {Path = new PropertyPath("Foreground"), Source = this});

                        AttachmentsPanel.Children.Add(forwardMessageControl);
                    }
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

        private void MessageForwardedMessageControl_OnSizeChanged(object sender, SizeChangedEventArgs e)
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
    }
}