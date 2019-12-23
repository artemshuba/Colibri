using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Colibri.Model;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Colibri.Controls
{
    public sealed partial class AttachmentUploadVideoControl : UserControl
    {
        public static readonly DependencyProperty RemoveCommandProperty = DependencyProperty.Register(
            "RemoveCommand", typeof(ICommand), typeof(AttachmentUploadVideoControl), new PropertyMetadata(default(ICommand)));

        public ICommand RemoveCommand
        {
            get { return (ICommand)GetValue(RemoveCommandProperty); }
            set { SetValue(RemoveCommandProperty, value); }
        }

        public static readonly DependencyProperty AttachmentProperty = DependencyProperty.Register(
            "Attachment", typeof(VideoAttachmentUpload), typeof(AttachmentUploadVideoControl), new PropertyMetadata(default(VideoAttachmentUpload), AttachmentPropertyChanged));

        private static void AttachmentPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (AttachmentUploadVideoControl)d;

            VisualStateManager.GoToState(control, "UploadingState", true);

            var newAttachment = (VideoAttachmentUpload)e.NewValue;
            var oldAttachment = (VideoAttachmentUpload)e.OldValue;

            if (oldAttachment != null)
                oldAttachment.PropertyChanged -= control.NewAttachment_PropertyChanged;

            if (newAttachment != null)
                newAttachment.PropertyChanged += control.NewAttachment_PropertyChanged;
        }

        public VideoAttachmentUpload Attachment
        {
            get { return (VideoAttachmentUpload)GetValue(AttachmentProperty); }
            set { SetValue(AttachmentProperty, value); }
        }

        public AttachmentUploadVideoControl()
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
