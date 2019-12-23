using System;
using System.Linq;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Colibri.Helpers;
using Jupiter.Utils.Extensions;

namespace Colibri.Controls
{
    public class ChatListView : ListView
    {
        private ScrollViewer _scrollViewer;
        private bool _autoScroll = true;
        private ISupportTopIncrementalLoading _incrementalTopLoadingCollection;
        private double _previousScrollOffset;

        private DateTime _lastRequest;

        private bool _isScrollSuccess = true;

        public ScrollViewer ScrollViewer
        {
            get { return _scrollViewer; }
        }

        public ChatListView()
        {
            this.Loaded += ChatListView_Loaded;
            this.SizeChanged += ChatListView_SizeChanged;
        }

        private void ChatListView_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            _scrollViewer = this.GetVisualDescendents().OfType<ScrollViewer>().First();
            _scrollViewer.ViewChanged += ScrollViewerViewChanged;

            if (_autoScroll && !_isScrollSuccess)
                ScrollToBottom();
        }

        private void ChatListView_SizeChanged(object sender, Windows.UI.Xaml.SizeChangedEventArgs e)
        {
            if (_scrollViewer == null)
                return;

            if (_scrollViewer.VerticalOffset >= _scrollViewer.ScrollableHeight - 10)
                _scrollViewer.ChangeView(null, _scrollViewer.ScrollableHeight, null, true);
        }

        private void ScrollViewerViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (_scrollViewer.VerticalOffset < _previousScrollOffset)
            {
                _autoScroll = _scrollViewer.VerticalOffset >= _scrollViewer.ScrollableHeight - 10; //оставляем небольшую погрешность

                if (_scrollViewer.VerticalOffset <= _scrollViewer.ScrollableHeight / 2 && _incrementalTopLoadingCollection != null &&
                    _incrementalTopLoadingCollection.HasMoreItemsOnTop)
                {
                    if ((DateTime.Now - _lastRequest).TotalMilliseconds > 1000)
                    {
                        _incrementalTopLoadingCollection.LoadMoreItemsOnTopAsync(50);

                        _lastRequest = DateTime.Now;
                    }
                }
            }

            _previousScrollOffset = _scrollViewer.VerticalOffset;
            // Debug.WriteLine(_scrollViewer.VerticalOffset);
        }

        protected override void OnItemsChanged(object e)
        {
            base.OnItemsChanged(e);

            if (_scrollViewer != null)
                _autoScroll = _scrollViewer.VerticalOffset >= _scrollViewer.ScrollableHeight - 100; //оставляем небольшую погрешность

            ScrollToBottom();
        }

        private void ScrollToBottom()
        {
            if (!DesignMode.DesignModeEnabled)
            {
                _incrementalTopLoadingCollection = ItemsSource as ISupportTopIncrementalLoading;

                //await Task.Delay(100);
                if (_autoScroll)
                {
                    //this.UpdateLayout();
                    //   _scrollViewer.ChangeView(null, _scrollViewer.ScrollableHeight, null, true);

                    var lastItem = Items?.LastOrDefault();
                    if (lastItem != null)
                    {
                        this.ScrollIntoView(lastItem, ScrollIntoViewAlignment.Leading);
                        _isScrollSuccess = _scrollViewer?.ChangeView(null, _scrollViewer.ScrollableHeight, null, true) ?? false;
                    }
                }
            }
        }
    }
}
