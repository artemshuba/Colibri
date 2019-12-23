using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using Windows.UI.Xaml.Media;
using Jupiter.Mvvm;
using VkLib.Core.Attachments;
using VkLib.Core.Photos;
using VkLib.Core.Video;

namespace Colibri.Model
{
    public class AttachmentUpload : BindableBase
    {
        private double _progress;
        private bool _isUploading;
        private bool _isUploaded;

        private CancellationTokenSource _token = new CancellationTokenSource();

        public double Progress
        {
            get { return _progress; }
            set { Set(ref _progress, value); }
        }

        public bool IsUploading
        {
            get { return _isUploading; }
            set { Set(ref _isUploading, value); }
        }

        public bool IsUploaded
        {
            get { return _isUploaded; }
            set { Set(ref _isUploaded, value); }
        }

        public CancellationTokenSource CancellationToken
        {
            get { return _token; }
        }

        public string FileName { get; set; }

        public Stream Stream { get; set; }
    }

    public class PhotoAttachmentUpload : AttachmentUpload
    {
        public ImageSource Photo { get; set; }

        public VkPhoto VkPhoto { get; set; }
    }

    public class DocumentAttachmentUpload : AttachmentUpload
    {
        private VkDocumentAttachment _vkDocument;

        public VkDocumentAttachment VkDocument
        {
            get { return _vkDocument; }
            set { Set(ref _vkDocument, value); }
        }
    }

    public class VideoAttachmentUpload : AttachmentUpload
    {
        private VkVideo _vkVideo;

        public VkVideo VkVideo
        {
            get { return _vkVideo; }
            set { Set(ref _vkVideo, value); }
        }
    }

    public class ForwardMessagesAttachmentUpload : AttachmentUpload
    {
        private ObservableCollection<Message> _messages = new ObservableCollection<Message>();

        public ObservableCollection<Message> Messages
        {
            get { return _messages; }
            set { Set(ref _messages, value); }
        }
    }
}