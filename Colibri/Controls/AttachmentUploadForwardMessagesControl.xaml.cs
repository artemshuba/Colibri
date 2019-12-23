using System.ComponentModel;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Colibri.Helpers;
using Colibri.Model;
using Jupiter.Utils.Helpers;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Colibri.Controls
{
    public sealed partial class AttachmentUploadForwardMessagesControl : UserControl
    {
        public static readonly DependencyProperty RemoveCommandProperty = DependencyProperty.Register(
                    "RemoveCommand", typeof(ICommand), typeof(AttachmentUploadForwardMessagesControl), new PropertyMetadata(default(ICommand)));

        public ICommand RemoveCommand
        {
            get { return (ICommand)GetValue(RemoveCommandProperty); }
            set { SetValue(RemoveCommandProperty, value); }
        }

        public static readonly DependencyProperty AttachmentProperty = DependencyProperty.Register(
            "Attachment", typeof(ForwardMessagesAttachmentUpload), typeof(AttachmentUploadForwardMessagesControl), new PropertyMetadata(default(ForwardMessagesAttachmentUpload), AttachmentPropertyChanged));

        private static void AttachmentPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (AttachmentUploadForwardMessagesControl)d;

            VisualStateManager.GoToState(control, "UploadingState", true);

            var newAttachment = (ForwardMessagesAttachmentUpload)e.NewValue;
            var oldAttachment = (ForwardMessagesAttachmentUpload)e.OldValue;

            if (oldAttachment != null)
                oldAttachment.PropertyChanged -= control.NewAttachment_PropertyChanged;

            if (newAttachment != null)
            {
                newAttachment.PropertyChanged += control.NewAttachment_PropertyChanged;

                control.UpdateCountTitle();
            }
        }

        public ForwardMessagesAttachmentUpload Attachment
        {
            get { return (ForwardMessagesAttachmentUpload)GetValue(AttachmentProperty); }
            set { SetValue(AttachmentProperty, value); }
        }

        public AttachmentUploadForwardMessagesControl()
        {
            this.InitializeComponent();
        }

        private void ContextMenuRemoveClick(object sender, RoutedEventArgs e)
        {
            Attachment.CancellationToken.Cancel();

            RemoveCommand?.Execute(Attachment);
        }

        private void AttachmentUploadForwardMessagesControl_OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (Attachment != null)
                Attachment.PropertyChanged -= NewAttachment_PropertyChanged;
        }

        private void NewAttachment_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //if (e.PropertyName == "IsUploaded" && Attachment.IsUploaded)
            //{
            //    VisualStateManager.GoToState(this, "UploadedState", true);
            //}
        }

        private void UpdateCountTitle()
        {
            MessagesCountTitle.Text = StringHelper.LocalizeNumerals(Attachment.Messages.Count,
                Localizator.String("MessagesSingular"), Localizator.String("MessagesDual"),
                Localizator.String("MessagesPlural")).ToLower();
        }
    }
}
