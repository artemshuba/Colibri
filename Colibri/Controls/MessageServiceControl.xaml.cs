using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Colibri.Model;
using VkLib.Core.Messages;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Colibri.Controls
{
    public sealed partial class MessageServiceControl : UserControl
    {
        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
            "Message", typeof(Message), typeof(MessageServiceControl), new PropertyMetadata(default(Message), MessagePropertyChanged));

        private static void MessagePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (MessageServiceControl)d;
            var message = e.NewValue as Message;
            if (message != null && message.MessageContent.Action != null)
                control.MessageTextBlock.Text = message.GetActionText();
        }

        public VkMessage Message
        {
            get { return (VkMessage)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        public MessageServiceControl()
        {
            this.InitializeComponent();
        }
    }
}
