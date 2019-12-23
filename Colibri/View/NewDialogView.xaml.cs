using Windows.UI.Xaml.Controls;
using Colibri.ViewModel;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Colibri.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NewDialogView : Page
    {
        public NewDialogView()
        {
            this.InitializeComponent();
        }

        private void AutoSuggestBox_OnSuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs e)
        {
            ((NewDialogViewModel)DataContext).StartChatCommand.Execute(e.SelectedItem);
        }
    }
}
