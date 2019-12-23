using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Colibri.Controls;
using Colibri.View;
using Colibri.ViewModel;
using Jupiter.Application;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Colibri
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private string _lastState = "Default";

        public MainPage()
        {
            this.InitializeComponent();

            if (ContentFrame != null)
            {
                JupiterApp.Current.NavigationServices.Register("Content", ContentFrame);
            }

#if MOCK
            this.DataContext = new Colibri.DebugData.DebugDialogsViewModel();
#endif
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            Window.Current.SetTitleBar(FakeTitleBar);
            ViewModelLocator.Main.OnNavigatedTo(null, e.NavigationMode);

            SystemNavigationManager.GetForCurrentView().BackRequested += MainPage_BackRequested;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            Window.Current.SetTitleBar(null);

            SystemNavigationManager.GetForCurrentView().BackRequested -= MainPage_BackRequested;

            base.OnNavigatingFrom(e);
        }

        private void MainPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (_lastState == "NarrowChatState")
                GoBack();
        }


        private void AdaptiveStates_OnCurrentStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            //if (e.NewState?.Name == "NarrowChatState")
            //{
            //    Window.Current.SetTitleBar(FakeTitleBar);
            //}
            //else if (e.OldState?.Name == "NarrowChatState")
            //    Window.Current.SetTitleBar(null);

            _lastState = e.NewState?.Name;
        }

        private void DialogsList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DialogsList.SelectedItem != null)
            {
                ContentFrame.Visibility = Visibility.Visible;
                if (this.Frame.ActualWidth < 720)
                {
                    VisualStateManager.GoToState(this, "NarrowChatState", true);
                }
                else
                    VisualStateManager.GoToState(this, "Default", true);
            }
            else
            {
                ContentFrame.Visibility = Visibility.Collapsed;
                if (this.Frame.ActualWidth < 720)
                {
                    VisualStateManager.GoToState(this, "NarrowState", true);
                }
            }
        }

        private void MainPage_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width < 720 && (e.PreviousSize.Width >= 720 || e.PreviousSize.Width == 0))
            {
                if (DialogsList.SelectedItem != null)
                    VisualStateManager.GoToState(this, "NarrowChatState", true);
                else
                    VisualStateManager.GoToState(this, "NarrowState", true);
            }
            else if (e.NewSize.Width >= 720 && e.PreviousSize.Width < 720)
                VisualStateManager.GoToState(this, "Default", true);
        }

        private void BackButton_OnClick(object sender, RoutedEventArgs e)
        {
            GoBack();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settings = new SettingsFlyoutControl();
            settings.FlyoutContent = new SettingsView();
            settings.Show();
        }

        private void GoBack()
        {
            ContentFrame.Navigate(typeof(Page), null, new SuppressNavigationTransitionInfo());
            ContentFrame.BackStack.Clear();

            DialogsList.SetValue(ListView.SelectedItemProperty, null);
        }
    }
}