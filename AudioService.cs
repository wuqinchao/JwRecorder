using NAudio.Wave;
using NAudio.Mixer;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JwRecorder
{
    public sealed class AudioService
    {
        /// <summary>
        /// 音频采样率 [8000,16000,32000,48000]
        /// </summary>
        public const int SampleRate = 48000;
        public static readonly WaveFormat Format = new WaveFormat(SampleRate, 1);
        public static readonly AudioService Service = null;
        private UnsignedMixerControl _volumeControl;
        private double _desiredVolume = 100;
        private int _device;
        /// <summary>
        /// 音频设备
        /// </summary>
        private WaveInEvent _waveIn;
        /// <summary>
        /// 运行状态
        /// </summary>
        private bool _running = false;
        private readonly object _locker = new object();

        #region 事件
        /// <summary>
        /// 收到音频数据事件
        /// </summary>
        public event EventHandler<byte[]> DataAvailable;
        /// <summary>
        /// 服务已停止事件
        /// </summary>
        public event EventHandler ServiceStopped = delegate { };
        /// <summary>
        /// 服务已启动事件
        /// </summary>
        public event EventHandler ServiceStarted = delegate { };
        #endregion
        public double MicrophoneLevel
        {
            get => _desiredVolume;
            set
            {
                _desiredVolume = value;
                if (_volumeControl != null)
                {
                    _volumeControl.Percent = value;
                }
            }
        }
        static AudioService()
        {
            Service = new AudioService();
        }
        private AudioService() { }
        public void Start(int recordingDevice)
        {
            Task t = new Task(new Action(() => {
                StartService(recordingDevice);
            }));
            t.Start();
        }
        private void StartService(int recordingDevice)
        {
            lock (_locker)
            {
                if (_running) return;
                _device = recordingDevice;
                while (!OpenDevice())
                {
                    Thread.Sleep(1000);
                    Console.WriteLine("音频设备启动失败,将于1秒后重试");
                }
            }
        }
        public void Stop()
        {
            lock (_locker)
            {
                _running = false;
                ClearDevice();
            }
        }
        private bool OpenDevice()
        {
            try
            {
                _waveIn = new WaveInEvent
                {
                    WaveFormat = Format,
                    DeviceNumber = _device
                };
                _waveIn.DataAvailable += OnDataAvailable;
                _waveIn.RecordingStopped += OnRecordingStopped;
                _waveIn.StartRecording();
                TryGetVolumeControl();
                _running = true;
            }
            catch (Exception e)
            {
                Console.WriteLine("OpenDevice失败", e);
                _running = false;
                return false;
            }

            Console.WriteLine("音频服务已启动");
            ServiceStarted?.Invoke(this, null);
            return true;
        }
        private void ClearDevice()
        {
            if (_waveIn == null) return;
            _waveIn.DataAvailable -= OnDataAvailable;
            _waveIn.RecordingStopped -= OnRecordingStopped;
            _waveIn.StopRecording();
            Console.WriteLine("音频服务已停止");
            _waveIn = null;
        }
        public void SetVolume(int volume = 100)
        {
            var waveInDeviceNumber = _waveIn.DeviceNumber;
            if (Environment.OSVersion.Version.Major >= 6) // Vista and over
            {
                var mixerLine = _waveIn.GetMixerLine();
                //new MixerLine((IntPtr)waveInDeviceNumber, 0, MixerFlags.WaveIn);
                foreach (var control in mixerLine.Controls)
                {
                    if (control.ControlType != MixerControlType.Volume) continue;
                    _volumeControl = control as UnsignedMixerControl;
                    MicrophoneLevel = volume;
                    break;
                }
            }
            else
            {
                var mixer = new Mixer(waveInDeviceNumber);
                foreach (var destination in mixer.Destinations
                    .Where(d => d.ComponentType == MixerLineComponentType.DestinationWaveIn))
                {
                    foreach (var source in destination.Sources
                        .Where(source => source.ComponentType == MixerLineComponentType.SourceMicrophone))
                    {
                        foreach (var control in source.Controls
                            .Where(control => control.ControlType == MixerControlType.Volume))
                        {
                            _volumeControl = control as UnsignedMixerControl;
                            MicrophoneLevel = volume;
                            break;
                        }
                    }
                }
            }
        }
        private void TryGetVolumeControl()
        {
            var waveInDeviceNumber = _waveIn.DeviceNumber;
            if (Environment.OSVersion.Version.Major >= 6) // Vista and over
            {
                var mixerLine = _waveIn.GetMixerLine();
                //new MixerLine((IntPtr)waveInDeviceNumber, 0, MixerFlags.WaveIn);
                foreach (var control in mixerLine.Controls)
                {
                    if (control.ControlType != MixerControlType.Volume) continue;
                    _volumeControl = control as UnsignedMixerControl;
                    MicrophoneLevel = _desiredVolume;
                    break;
                }
            }
            else
            {
                var mixer = new Mixer(waveInDeviceNumber);
                foreach (var destination in mixer.Destinations
                    .Where(d => d.ComponentType == MixerLineComponentType.DestinationWaveIn))
                {
                    foreach (var source in destination.Sources
                        .Where(source => source.ComponentType == MixerLineComponentType.SourceMicrophone))
                    {
                        foreach (var control in source.Controls
                            .Where(control => control.ControlType == MixerControlType.Volume))
                        {
                            _volumeControl = control as UnsignedMixerControl;
                            MicrophoneLevel = _desiredVolume;
                            break;
                        }
                    }
                }
            }
        }
        private void OnRecordingStopped(object sender, StoppedEventArgs e)
        {
            ServiceStopped?.Invoke(this, null);
            lock (_locker)
            {
                if (!_running) return;
                Console.WriteLine("音频设备意外停止", e.Exception);
                ClearDevice();
                while (!OpenDevice())
                {
                    Thread.Sleep(500);
                    Console.WriteLine("音频设备启动失败,将于0.5秒后重试");
                }
            }
        }
        private void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            DataAvailable?.Invoke(sender, e.Buffer);
        }
    }
}
