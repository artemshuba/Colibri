using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Colibri.Services;
using VkLib.Core.Attachments;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Colibri.Controls
{
    public sealed partial class MessageAudioControl : UserControl
    {
        private static MessageAudioControl _activeControl = null;
        private bool _notifyProgressBar = true;

        public static readonly DependencyProperty AudioProperty = DependencyProperty.Register(
            "Audio", typeof(VkAudioAttachment), typeof(MessageAudioControl), new PropertyMetadata(default(VkAudioAttachment), OnAudioPropertyChanged));

        private static void OnAudioPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (MessageAudioControl)d;

            if (e.NewValue == _activeControl)
                control.SubscribeAudioEvents();
            else
                control.UnsubscribeAudioEvents();
        }

        public VkAudioAttachment Audio
        {
            get { return (VkAudioAttachment)GetValue(AudioProperty); }
            set { SetValue(AudioProperty, value); }
        }

        public static readonly DependencyProperty IsPlayingProperty = DependencyProperty.Register(
            "IsPlaying", typeof(bool), typeof(MessageAudioControl), new PropertyMetadata(default(bool), OnIsPlayingPropertyChanged));

        private static void OnIsPlayingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (MessageAudioControl)d;

            if ((bool)e.NewValue)
            {
                control.PlayIcon.Opacity = 0;
                control.PauseIcon.Opacity = 1;

                control.TitleTextBlock.FontSize -= 2;
                control.DurationTextBlock.FontSize -= 2;

                control.ProgressBar.Visibility = Visibility.Visible;

                control.SubscribeAudioEvents();
            }
            else
            {
                control.PauseIcon.Opacity = 0;
                control.PlayIcon.Opacity = 1;

                control.TitleTextBlock.FontSize += 2;
                control.DurationTextBlock.FontSize += 2;

                control.ProgressBar.Visibility = Visibility.Collapsed;

                control.UnsubscribeAudioEvents();
            }
        }

        public bool IsPlaying
        {
            get { return (bool)GetValue(IsPlayingProperty); }
            set { SetValue(IsPlayingProperty, value); }
        }

        public MessageAudioControl()
        {
            this.InitializeComponent();
        }

        private void RootButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (_activeControl == this)
            {
                if (!IsPlaying)
                    ServiceLocator.AudioService.Play();
                else
                    ServiceLocator.AudioService.Pause();
            }
            else
            {
                ServiceLocator.AudioService.PlayAudio(Audio);
            }

            IsPlaying = !IsPlaying;

            if (IsPlaying)
            {
                if (_activeControl != null)
                    _activeControl.IsPlaying = false;

                _activeControl = this;
            }
            else if (_activeControl == this)
            {
                _activeControl = null;
            }
        }

        private void SubscribeAudioEvents()
        {
            ServiceLocator.AudioService.PositionChanged += AudioServiceOnPositionChanged;

            //if (IsPlaying)
            //    ServiceLocator.AudioService.PlayStateChanged += AudioServiceOnPlayStateChanged;
        }

        private void UnsubscribeAudioEvents()
        {
            ServiceLocator.AudioService.PositionChanged -= AudioServiceOnPositionChanged;
            //ServiceLocator.AudioService.PlayStateChanged -= AudioServiceOnPlayStateChanged;
        }

        private void AudioServiceOnPositionChanged(object sender, TimeSpan timeSpan)
        {
            _notifyProgressBar = false;
            ProgressBar.Value = timeSpan.TotalSeconds;
            _notifyProgressBar = true;
        }

        private void AudioServiceOnPlayStateChanged(object sender, EventArgs e)
        {
            var newValue = ServiceLocator.AudioService.IsPlaying;
            if (IsPlaying != newValue)
                IsPlaying = newValue;
        }

        private void ProgressBar_OnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (!_notifyProgressBar)
                return;

            ServiceLocator.AudioService.Seek(TimeSpan.FromSeconds(e.NewValue));
        }
    }
}