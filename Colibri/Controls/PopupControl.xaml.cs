using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Jupiter.Utils.Extensions;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Colibri.Controls
{
    public sealed partial class PopupControl : UserControl
    {
        private object _result = null;

        public static readonly DependencyProperty FlyoutContentProperty =
            DependencyProperty.Register("FlyoutContent", typeof(object), typeof(PopupControl), new PropertyMetadata(default(object)));

        public object FlyoutContent
        {
            get { return (object)GetValue(FlyoutContentProperty); }
            set { SetValue(FlyoutContentProperty, value); }
        }

        public static readonly DependencyProperty FlyoutContentTemplateProperty =
            DependencyProperty.Register("FlyoutContentTemplate", typeof(DataTemplate), typeof(PopupControl), new PropertyMetadata(default(DataTemplate)));

        public DataTemplate FlyoutContentTemplate
        {
            get { return (DataTemplate)GetValue(FlyoutContentTemplateProperty); }
            set { SetValue(FlyoutContentTemplateProperty, value); }
        }

        public delegate void ClosedEventHandler(object result);
        public event ClosedEventHandler Closed;

        public PopupControl()
        {
            InitializeComponent();
        }

        public void Show(bool useTransitions = true)
        {
            var mainWindow = Window.Current;
            if (mainWindow.Content == null)
                return;

            var panel = ((Frame)mainWindow.Content).GetVisualDescendents().OfType<Panel>().FirstOrDefault();
            if (panel == null)
            {
                return;
            }

            var page = ((Frame)mainWindow.Content).Content as Page;
            if (page != null)
            {
                if (page.TopAppBar != null)
                {
                    page.TopAppBar.Opened += AppBar_Opened;
                    page.TopAppBar.IsOpen = false;
                }

                if (page.BottomAppBar != null)
                {
                    page.BottomAppBar.Opened += AppBar_Opened;
                    page.BottomAppBar.IsOpen = false;
                }
            }

            panel.Children.Add(this);

            if (useTransitions)
            {
                var s = ((Storyboard)Resources["LoadAnim"]);
                s.Begin();
            }

            Window.Current.SetTitleBar(TitleBar);
        }

        public Task<object> ShowAsync(bool useTransitions = true)
        {
            var tcs = new TaskCompletionSource<object>();

            Show(useTransitions);
            Closed += (result) => tcs.TrySetResult(result);

            return tcs.Task;
        }

        public void Close(object result = null)
        {
            _result = result;

            var s = ((Storyboard)Resources["CloseAnim"]);
            s.Begin();

            Window.Current.SetTitleBar(null);
        }

        public static void CloseCurrent()
        {
            var mainWindow = Window.Current;
            if (mainWindow.Content == null)
                return;

            var flyoutControl = ((Frame)mainWindow.Content).GetVisualDescendents().OfType<PopupControl>().FirstOrDefault();
            if (flyoutControl == null)
            {
                return;
            }

            flyoutControl.Close();
        }

        public void CloseNow(object result = null)
        {
            _result = result;

            CloseInternal();
        }

        private void CloseInternal()
        {
            var mainWindow = Window.Current;

            if (mainWindow.Content == null)
                return;

            var panel = ((Frame)mainWindow.Content).GetVisualDescendents().OfType<Panel>().FirstOrDefault();
            if (panel == null)
            {
                return;
            }

            var page = ((Frame)mainWindow.Content).Content as Page;
            if (page != null)
            {
                if (page.TopAppBar != null)
                    page.TopAppBar.Opened -= AppBar_Opened;
                if (page.BottomAppBar != null)
                    page.BottomAppBar.Opened -= AppBar_Opened;
            }

            panel.Children.Remove(this);

            if (Closed != null)
                Closed(_result);
        }

        private void AppBar_Opened(object sender, object e)
        {
            var appbar = (AppBar)sender;
            appbar.IsOpen = false; //prevent appbar from opening
        }

        private void CloseAnim_OnCompleted(object sender, object e)
        {
            CloseInternal();
        }

        private void Overlay_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            Close();
        }

        private void BackButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void PopupControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().BackRequested += PopupControl_BackRequested;
        }

        private void PopupControl_OnUnloaded(object sender, RoutedEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().BackRequested -= PopupControl_BackRequested;
        }

        private void PopupControl_BackRequested(object sender, BackRequestedEventArgs e)
        {
            CloseCurrent();
        }
    }
}
