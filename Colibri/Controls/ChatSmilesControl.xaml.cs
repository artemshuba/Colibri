using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Input;
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Colibri.Helpers;
using Colibri.Services;
using Jupiter.Utils.Extensions;
using Jupiter.Utils.Helpers;
using VkLib.Core.Store;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Colibri.Controls
{
    public sealed partial class ChatSmilesControl : UserControl
    {
        private class StickerItem
        {
            public string ImageUrl { get; set; }

            public int Id { get; set; }
        }


        private List<VkStoreProduct> _stickers;
        private VkStickerPackProduct _recentStickers;

        public event EventHandler<string> EmojiChoosenEvent;
        public event EventHandler<VkStickerProduct> StickerChoosenEvent;

        public ChatSmilesControl()
        {
            this.InitializeComponent();
        }

        private void ChatSmilesControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            LoadStickers();
        }

        private async void InitEmojis()
        {
            ContentHost.Children.Clear();

            var scrollViewer = new ScrollViewer();
            scrollViewer.HorizontalScrollMode = ScrollMode.Disabled;
            scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            scrollViewer.VerticalScrollMode = ScrollMode.Auto;
            scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;

            var emojiGrid = new Grid();
            scrollViewer.Content = emojiGrid;

            var highlightsCanvas = new Canvas();
            emojiGrid.Children.Add(highlightsCanvas);

            var emojiSpriteImage = new Image();
            emojiSpriteImage.Margin = new Thickness(7);
            emojiSpriteImage.Width = 295;
            emojiSpriteImage.Height = 1576;
            emojiSpriteImage.IsHitTestVisible = false;
            emojiSpriteImage.Stretch = Stretch.Fill;

            var scale = DeviceHelper.ResolutionScale;
            if ((int)scale >= 150)
                emojiSpriteImage.Source = new BitmapImage(new Uri("ms-appx:///Resources/Images/Smiles/sprite180.png"));
            else if ((int)scale >= 120)
                emojiSpriteImage.Source = new BitmapImage(new Uri("ms-appx:///Resources/Images/Smiles/sprite140.png"));
            else
                emojiSpriteImage.Source = new BitmapImage(new Uri("ms-appx:///Resources/Images/Smiles/sprite.png"));

            emojiGrid.Children.Add(emojiSpriteImage);

            ContentHost.Children.Add(scrollViewer);

            int r = 0, c = 0;
            foreach (var smile in Smiles.Base.Keys)
            {
                var highlightCanvas = new Canvas();
                highlightCanvas.Width = 31;
                highlightCanvas.Height = 30;
                highlightCanvas.Background = (Brush)Application.Current.Resources["ApplicationForegroundThemeBrush"];
                highlightCanvas.Opacity = 0;
                highlightCanvas.AddHandler(PointerEnteredEvent, new PointerEventHandler((s, args) =>
                {
                    if (args.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
                        highlightCanvas.Opacity = 0.3;
                }), false);
                highlightCanvas.AddHandler(PointerExitedEvent, new PointerEventHandler((s, args) =>
                {
                    if (args.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
                        highlightCanvas.Opacity = 0;
                }), false);
                highlightCanvas.AddHandler(PointerPressedEvent, new PointerEventHandler((s, args) =>
                {
                    if (args.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
                        highlightCanvas.Opacity = 0.2;
                }), false);
                highlightCanvas.AddHandler(PointerReleasedEvent, new PointerEventHandler((s, args) =>
                {
                    if (args.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
                        highlightCanvas.Opacity = 0.3;
                }), false);
                highlightCanvas.AddHandler(TappedEvent, new TappedEventHandler((s, args) =>
                {
                    EmojiChoosenEvent?.Invoke(this, smile);
                }), false);


                Canvas.SetLeft(highlightCanvas, c * 31);
                Canvas.SetTop(highlightCanvas, r * 30);

                if (highlightsCanvas.Children.Count != 528)
                {
                    ++c;
                    if (c == 10)
                    {
                        c = 0;
                        ++r;

                        await Task.Delay(1);
                    }

                    highlightsCanvas.Children.Add(highlightCanvas);
                }
                else
                    break;
            }
        }

        private void InitStickers(int stickerPackIndex)
        {
            ContentHost.Children.Clear();

            var stickersListView = new ListView();

            stickersListView.ItemTemplate = (DataTemplate)Resources["StickerItemTemplate"];
            stickersListView.ItemsPanel = (ItemsPanelTemplate)Resources["StickersPanelTemplate"];
            stickersListView.ItemContainerStyle = (Style)Resources["StickerListViewItemStyle"];

            VkStickerPackProduct stickerPack;
            if (_recentStickers != null)
            {
                if (stickerPackIndex == 0)
                    stickerPack = _recentStickers;
                else
                    stickerPack = _stickers[stickerPackIndex - 1].Stickers;
            }
            else
            {
                stickerPack = _stickers[stickerPackIndex].Stickers;
            }

            var stickersSource = stickerPack.StickerIds.Select(id => new StickerItem() { Id = id, ImageUrl = stickerPack.BaseUrl + id + "/128.png" }).ToList();

            stickersListView.ItemsSource = stickersSource;

            ScrollViewer.SetVerticalScrollBarVisibility(stickersListView, ScrollBarVisibility.Hidden);
            stickersListView.HorizontalAlignment = HorizontalAlignment.Center;
            stickersListView.SelectionMode = ListViewSelectionMode.None;

            ContentHost.Children.Add(stickersListView);
        }

        private async void LoadStickers()
        {
            if (_recentStickers == null)
            {
                try
                {
                    var recentStickersResult = await ServiceLocator.Vkontakte.Execute.GetRecentStickers();
                    if (recentStickersResult != null)
                    {
                        _recentStickers = recentStickersResult;

                        var textBlock = new TextBlock();
                        textBlock.Text = "";
                        textBlock.FontFamily = (FontFamily)Application.Current.Resources["SymbolThemeFontFamily"];
                        textBlock.Opacity = 0.6;
                        TabsListView.Items.Add(textBlock);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Unable to load recent stickers");
                }
            }

            if (!_stickers.IsNullOrEmpty())
                return;

            try
            {
                var result = await ServiceLocator.Vkontakte.Store.GetProducts("stickers", "active");
                if (result != null && !result.Items.IsNullOrEmpty())
                {
                    _stickers = result.Items;
                    foreach (var stickerPack in result.Items)
                    {
                        var stickerPackCover = new Image();
                        stickerPackCover.Width = 22;
                        stickerPackCover.Height = 22;
                        stickerPackCover.Source = new BitmapImage(new Uri(stickerPack.BaseUrl + "thumb_44.png"));
                        TabsListView.Items.Add(stickerPackCover);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to load stickers");
            }
        }

        private void TabsListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TabsListView.SelectedIndex == 0)
                InitEmojis();
            else
                InitStickers(TabsListView.SelectedIndex - 1);
        }

        private void StickerItemClick(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;

            var stickerPackIndex = TabsListView.SelectedIndex;

            VkStickerPackProduct stickerPack;
            if (_recentStickers != null)
            {
                if (stickerPackIndex == 1)
                    stickerPack = _recentStickers;
                else
                    stickerPack = _stickers[stickerPackIndex - 2].Stickers;
            }
            else
            {
                stickerPack = _stickers[stickerPackIndex - 1].Stickers;
            }

            StickerChoosenEvent?.Invoke(this, new VkStickerProduct() { Id = (int)button.Tag, BaseUrl = stickerPack.BaseUrl });
        }
    }
}