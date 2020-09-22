using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JwRecorder
{
    public class AudioPlayer
    {
        private WaveOut waveOut;
        private MediaFoundationReader reader;

        public void LoadFile(string path)
        {
            CloseWaveOut();
            CloseInStream();
            reader = new MediaFoundationReader(path);
        }

        public void Play()
        {
            CreateWaveOut();
            if (waveOut.PlaybackState == PlaybackState.Stopped)
            {
                reader.CurrentTime = TimeSpan.Zero;
                waveOut.Play();
            }
        }

        private void CreateWaveOut()
        {
            if (waveOut == null)
            {
                waveOut = new WaveOut();
                waveOut.Init(reader);
                waveOut.PlaybackStopped += OnPlaybackStopped;
            }
        }

        void OnPlaybackStopped(object sender, StoppedEventArgs e)
        {
            PlaybackState = PlaybackState.Stopped;
        }

        public void Stop()
        {
            if (waveOut != null)
            {
                waveOut.Stop();
                reader.CurrentTime = TimeSpan.Zero;
            }
        }

        public TimeSpan CurrentPosition { get; set; }
        public PlaybackState PlaybackState { get; private set; }

        public void Dispose()
        {
            CloseWaveOut();
            CloseInStream();
        }

        private void CloseInStream()
        {
            if (reader != null)
            {
                reader.Dispose();
                reader = null;
            }
        }

        private void CloseWaveOut()
        {
            if (waveOut != null)
            {
                waveOut.Dispose();
                waveOut = null;
            }
        }
    }
}
