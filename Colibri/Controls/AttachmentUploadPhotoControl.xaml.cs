using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Colibri.Helpers;
using Colibri.Model;
using Colibri.View;
using VkLib.Core.Attachments;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Colibri.Controls
{
    public sealed partial class AttachmentUploadPhotoControl : UserControl
    {
        public static readonly DependencyProperty RemoveCommandProperty = DependencyProperty.Register(
            "RemoveCommand", typeof(ICommand), typeof(AttachmentUploadPhotoControl), new PropertyMetadata(default(ICommand)));

        public ICommand RemoveCommand
        {
            get { return (ICommand)GetValue(RemoveCommandProperty); }
            set { SetValue(RemoveCommandProperty, value); }
        }

        public static readonly DependencyProperty AttachmentProperty = DependencyProperty.Register(
            "Attachment", typeof(PhotoAttachmentUpload), typeof(AttachmentUploadPhotoControl), new PropertyMetadata(default(PhotoAttachmentUpload), AttachmentPropertyChanged));

        private static void AttachmentPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (AttachmentUploadPhotoControl)d;

            VisualStateManager.GoToState(control, "UploadingState", true);

            var newAttachment = (PhotoAttachmentUpload)e.NewValue;
            var oldAttachment = (PhotoAttachmentUpload)e.OldValue;

            if (oldAttachment != null)
                oldAttachment.PropertyChanged -= control.NewAttachment_PropertyChanged;

            if (newAttachment != null)
                newAttachment.PropertyChanged += control.NewAttachment_PropertyChanged;
        }

        public PhotoAttachmentUpload Attachment
        {
            get { return (PhotoAttachmentUpload)GetValue(AttachmentProperty); }
            set { SetValue(AttachmentProperty, value); }
        }

        public AttachmentUploadPhotoControl()
        {
            this.InitializeComponent();
        }

        private void ContextMenuRemoveClick(object sender, RoutedEventArgs e)
        {
            Attachment.CancellationToken.Cancel();

            RemoveCommand?.Execute(Attachment);
        }

        private void AttachmentUploadPhotoControl_OnUnloaded(object sender, RoutedEventArgs e)
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

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var p = new Dictionary<string, object>();
            p.Add("currentPhotoSource", Attachment.Photo);
            Navigator.NavigateAdaptive(typeof(PhotosPreviewView), p);
        }
    }
}