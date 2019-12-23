using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Devices.Input;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Colibri.Helpers;
using Colibri.ViewModel;
using Jupiter.Utils.Helpers;
using VkLib.Core.Store;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Colibri.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ChatView : Page
    {
        private DateTime _lastMessagesMarkAsRead;

        public ChatView()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
#if MOCK
            this.DataContext = new Colibri.DebugData.DebugConversationViewModel();
#else            
            if (e.Parameter != null)
                this.DataContext = e.Parameter;
#endif
            base.OnNavigatedTo(e);

            //deferred loading
            if (DeviceHelper.IsMobile())
            {
                await Task.Delay(100);
                this.FindName("ChatListView");
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (ChatListView.ScrollViewer != null)
                ChatListView.ScrollViewer.ViewChanging -= ScrollViewer_ViewChanging;

            if (e.NavigationMode == NavigationMode.Back)
            {
                if (DeviceHelper.IsMobile())
                {
                    //clearing page cache
                    var cacheSize = ((Frame)Parent).CacheSize;
                    ((Frame)Parent).CacheSize = 0;
                    ((Frame)Parent).CacheSize = cacheSize;
                }
            }

            base.OnNavigatingFrom(e);
        }

        private void MessageTextBox_OnKeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter && (CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.Shift) & CoreVirtualKeyStates.Down) != CoreVirtualKeyStates.Down)
            {
                //send message
                ((ConversationViewModel)this.DataContext).SendMessageCommand.Execute(null);
            }
        }

        private void ChatListView_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (ChatListView.ScrollViewer != null)
                ChatListView.ScrollViewer.ViewChanging += ScrollViewer_ViewChanging;
        }

        private void ChatListView_OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            CheckAndMarkAsRead();
        }

        private void ScrollViewer_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            CheckAndMarkAsRead();
        }

        private void MessageTextBox_OnTextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            CheckAndMarkAsRead();
        }

        private void CheckAndMarkAsRead()
        {
            if ((DateTime.Now - _lastMessagesMarkAsRead).TotalSeconds < 5)
                return;

            ((ConversationViewModel)this.DataContext).UserReadMessageCommand.Execute(null);

            _lastMessagesMarkAsRead = DateTime.Now;
        }

        private void MessageTextBox_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (new KeyboardCapabilities().KeyboardPresent > 0)
                MessageTextBox.Focus(FocusState.Programmatic);
        }

        private async void ChatTextBoxGrid_OnDragEnter(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.Bitmap))
            {
                e.AcceptedOperation = DataPackageOperation.Copy;
                if (e.DragUIOverride != null)
                    e.DragUIOverride.Caption = Localizator.String("DragDropAttachImage");
                e.Handled = true;
            }
            else if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var deferral = e.GetDeferral();
                e.AcceptedOperation = DataPackageOperation.None;

                var items = await e.DataView.GetStorageItemsAsync();
                int filesCount = 0;

                foreach (var item in items)
                {
                    try
                    {
                        var file = item as StorageFile;
                        if (file != null)
                        {
                            //if (file.ContentType.StartsWith("image/"))
                            //    filesCount++;
                            //else
                            filesCount++;
                        }
                    }
                    catch { }
                }

                if (filesCount > 0)
                {
                    e.AcceptedOperation = DataPackageOperation.Copy;
                    if (e.DragUIOverride != null)
                    {
                        e.DragUIOverride.Caption = filesCount == 1 ? Localizator.String("DragDropAttachFile") : Localizator.String("DragDropAttachFiles");
                    }
                }

                e.Handled = true;
                deferral.Complete();
            }
        }

        private async void ChatTextBoxGrid_OnDrop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.Bitmap))
            {
                var deferral = e.GetDeferral();

                try
                {
                    var streamReference = await e.DataView.GetBitmapAsync();
                    var stream = await streamReference.OpenReadAsync();

                    ((ConversationViewModel)DataContext).AttachPhotoFromStream(stream.CloneStream(), stream.ContentType);
                    stream.Dispose();
                }
                catch { }

                e.Handled = true;
                deferral.Complete();
            }
            else if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var deferral = e.GetDeferral();

                var items = await e.DataView.GetStorageItemsAsync();

                foreach (var item in items)
                {
                    try
                    {
                        var file = item as StorageFile;
                        if (file != null)
                        {
                            if (file.ContentType.StartsWith("image/"))
                                ((ConversationViewModel)DataContext).AttachPhotoFromFile(file);
                            else if (file.ContentType.StartsWith("video/"))
                                ((ConversationViewModel)DataContext).AttachVideoFromFile(file);
                            else
                                ((ConversationViewModel)DataContext).AttachDocumentFromFile(file);
                        }
                    }
                    catch { }
                }

                e.Handled = true;
                deferral.Complete();
            }
        }

        private async void MessageTextBox_OnPaste(object sender, TextControlPasteEventArgs e)
        {
            var data = Clipboard.GetContent();
            if (data.Contains(StandardDataFormats.Bitmap))
            {
                try
                {
                    var streamReference = await data.GetBitmapAsync();
                    var stream = await streamReference.OpenReadAsync();

                    ((ConversationViewModel)DataContext).AttachPhotoFromStream(stream.CloneStream(), stream.ContentType);
                    stream.Dispose();
                }
                catch { }

                e.Handled = true;
            }
            else if (data.Contains(StandardDataFormats.StorageItems))
            {
                var items = await data.GetStorageItemsAsync();

                foreach (var item in items)
                {
                    try
                    {
                        var file = item as StorageFile;
                        if (file != null && file.ContentType.StartsWith("image/"))
                        {
                            ((ConversationViewModel)DataContext).AttachPhotoFromFile(file);
                        }
                    }
                    catch { }
                }

                e.Handled = true;
            }
        }

        private void ChatSmilesControl_OnEmojiChoosenEvent(object sender, string e)
        {
            int start = MessageTextBox.SelectionStart;
            var s = "";
            if (start > 0 && MessageTextBox.Text[start - 1] != 32)
                s = " ";
            s = s + e;
            if (start == MessageTextBox.Text.Length || MessageTextBox.Text[start] != 32)
                s += " ";

            MessageTextBox.Text = MessageTextBox.Text.Insert(start, s);
            MessageTextBox.SelectionStart = start + s.Length + 1;
        }

        private void ChatSmilesControl_OnStickerChoosenEvent(object sender, VkStickerProduct sticker)
        {
            EmojiFlyout.Hide();
            ((ConversationViewModel)DataContext).SendStickerCommand.Execute(sticker);
        }
    }
}