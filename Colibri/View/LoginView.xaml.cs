using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Colibri.ViewModel;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Colibri.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginView : Page
    {
        public LoginView()
        {
            this.InitializeComponent();
        }

        private void LoginBox_OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                if (!string.IsNullOrWhiteSpace(LoginBox.Text))
                {
                    if (string.IsNullOrWhiteSpace(PasswordBox.Password))
                    {
                        PasswordBox.Focus(FocusState.Keyboard);
                    }
                    else if (CaptchaForm.Visibility == Visibility.Visible && string.IsNullOrWhiteSpace(CaptchaBox.Text))
                    {
                        CaptchaBox.Focus(FocusState.Keyboard);
                    }
                    else
                    {

                        ((LoginViewModel)DataContext).LoginCommand.Execute(null);
                    }
                }
            }
        }

        private void PasswordBox_OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                if (!string.IsNullOrWhiteSpace(PasswordBox.Password))
                {
                    if (string.IsNullOrWhiteSpace(LoginBox.Text))
                    {
                        LoginBox.Focus(FocusState.Keyboard);
                    }
                    else if (CaptchaForm.Visibility == Visibility.Visible && string.IsNullOrWhiteSpace(CaptchaBox.Text))
                    {
                        CaptchaBox.Focus(FocusState.Keyboard);
                    }
                    else
                    {
                        ((LoginViewModel)DataContext).LoginCommand.Execute(null);
                    }
                }
            }
        }

        private void CaptchaBox_OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                if (!string.IsNullOrWhiteSpace(CaptchaBox.Text))
                {
                    if (string.IsNullOrWhiteSpace(LoginBox.Text))
                    {
                        LoginBox.Focus(FocusState.Keyboard);
                    }
                    else if (string.IsNullOrWhiteSpace(PasswordBox.Password))
                    {
                        PasswordBox.Focus(FocusState.Keyboard);
                    }
                    else
                    {
                        ((LoginViewModel)DataContext).LoginCommand.Execute(null);
                    }
                }
            }
        }
    }
}