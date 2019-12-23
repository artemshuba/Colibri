using System;
using Windows.Media.Playback;
using Windows.UI.Xaml;
using GalaSoft.MvvmLight.Threading;
using VkLib.Core.Attachments;

namespace Colibri.Services
{
    public class AudioService
    {
        private DispatcherTimer _positionTimer;

        //events
        public event EventHandler<TimeSpan> PositionChanged;
        public event EventHandler PlayStateChanged;

        //properties
        public bool IsPlaying => BackgroundMediaPlayer.Current.CurrentState == MediaPlayerState.Playing
            || BackgroundMediaPlayer.Current.CurrentState == MediaPlayerState.Opening
            || BackgroundMediaPlayer.Current.CurrentState == MediaPlayerState.Buffering;

        public TimeSpan Position => BackgroundMediaPlayer.Current.Position;

        public AudioService()
        {
            Initialize();
        }

        public void PlayAudio(VkAudioAttachment audio)
        {
            BackgroundMediaPlayer.Current.SetUriSource(new Uri(audio.Url));
            BackgroundMediaPlayer.Current.Play();

            _positionTimer.Start();
        }

        public void Play()
        {
            BackgroundMediaPlayer.Current.Play();

            _positionTimer.Start();
        }

        public void Pause()
        {
            BackgroundMediaPlayer.Current.Pause();

            _positionTimer.Stop();
        }

        public void Seek(TimeSpan position)
        {
            BackgroundMediaPlayer.Current.Position = position;
        }

        private void Initialize()
        {
            BackgroundMediaPlayer.Current.CurrentStateChanged += MediaPlayerOnCurrentStateChanged;
            //BackgroundMediaPlayer.Current.MediaEnded += MediaPlayerOnMediaEnded;

            _positionTimer = new DispatcherTimer();
            _positionTimer.Interval = TimeSpan.FromMilliseconds(500);
            _positionTimer.Tick += PositionTimerOnTick;
        }

        private void PositionTimerOnTick(object sender, object o)
        {
            PositionChanged?.Invoke(this, Position);
        }

        private void MediaPlayerOnCurrentStateChanged(MediaPlayer sender, object args)
        {
            Logger.Info(sender.CurrentState.ToString());

            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                if (sender.CurrentState == MediaPlayerState.Playing)
                    _positionTimer.Start();
                else
                    _positionTimer.Stop();

                PlayStateChanged?.Invoke(this, EventArgs.Empty);
            });
        }
    }
}