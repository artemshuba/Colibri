using System.ComponentModel;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Colibri.Model;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Colibri.Controls
{
    public sealed partial class AttachmentUploadDocumentControl : UserControl
    {
        public static readonly DependencyProperty RemoveCommandProperty = DependencyProperty.Register(
            "RemoveCommand", typeof(ICommand), typeof(AttachmentUploadDocumentControl), new PropertyMetadata(default(ICommand)));

        public ICommand RemoveCommand
        {
            get { return (ICommand)GetValue(RemoveCommandProperty); }
            set { SetValue(RemoveCommandProperty, value); }
        }

        public static readonly DependencyProperty AttachmentProperty = DependencyProperty.Register(
            "Attachment", typeof(DocumentAttachmentUpload), typeof(AttachmentUploadDocumentControl), new PropertyMetadata(default(DocumentAttachmentUpload), AttachmentPropertyChanged));

        private static void AttachmentPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (AttachmentUploadDocumentControl)d;

            VisualStateManager.GoToState(control, "UploadingState", true);

            var newAttachment = (DocumentAttachmentUpload)e.NewValue;
            var oldAttachment = (DocumentAttachmentUpload)e.OldValue;

            if (oldAttachment != null)
                oldAttachment.PropertyChanged -= control.NewAttachment_PropertyChanged;

            if (newAttachment != null)
                newAttachment.PropertyChanged += control.NewAttachment_PropertyChanged;
        }

        public DocumentAttachmentUpload Attachment
        {
            get { return (DocumentAttachmentUpload)GetValue(AttachmentProperty); }
            set { SetValue(AttachmentProperty, value); }
        }

        public AttachmentUploadDocumentControl()
        {
            this.InitializeComponent();
        }

        private void ContextMenuRemoveClick(object sender, RoutedEventArgs e)
        {
            Attachment.CancellationToken.Cancel();

            RemoveCommand?.Execute(Attachment);
        }

        private void AttachmentUploadDocumentControl_OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (Attachment != null)
                Attachment.PropertyChanged -= NewAttachment_PropertyChanged;
        }

        private void NewAttachment_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsUploaded" && Attachment.IsUploaded)
            {
                VisualStateManager.GoToState(this, "UploadedState", true);
            }
        }
    }
}