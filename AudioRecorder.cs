using System;
using System.Linq;
using NAudio.Wave;
using NAudio.Mixer;
using System.Threading.Tasks;
using System.IO;

namespace JwRecorder
{
    public delegate void RecorderStatusChangedHandle(object sender, bool recording, DateTime starTime, TimeSpan totalTime);
    /// <summary>
    /// 音频录制器
    /// </summary>
    public class AudioRecorder
    {
        public static readonly string ParentPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyMusic);
        private bool _running = false;
        /// <summary>
        /// 互斥锁
        /// </summary>
        private readonly object _locker = new object();
        private readonly string _uuid;
        private WaveFileWriter _writer;
        private readonly string _dir;
        private readonly string _wav;
        private readonly DateTime _starTime;
        private DateTime _lastAudioData = DateTime.MinValue;
        /// <summary>
        /// 录音状态变更
        /// </summary>
        public event RecorderStatusChangedHandle OnRecorderStatusChanged;

        public AudioRecorder()
        {
            _starTime = DateTime.Now;
            _uuid = _starTime.ToString($"yyMMddHHmmss");
            _dir = Path.Combine(ParentPath,"recorder");
            _wav = Path.Combine(_dir,$"{_uuid}.wav");
        }
        /// <summary>
        /// 运行状态
        /// </summary>
        public bool Running { get => _running; }
        /// <summary>
        /// 录音编号
        /// </summary>
        public string Uuid { get => _uuid; }
        /// <summary>
        /// 录音目录
        /// </summary>
        public string Dir { get => _dir; }
        public string File => _wav;
        /// <summary>
        /// 录音开始时间
        /// </summary>
        public DateTime StarTime { get => _starTime; }
        public void Start()
        {
            var t = new Task(() =>
            {
                lock (_locker)
                {
                    if (_running) return;
                    if (!Directory.Exists(_dir))
                    {
                        Directory.CreateDirectory(_dir);
                        Console.WriteLine("创建目录{0}", _dir);
                    }
                    Console.WriteLine("开始启动录音");
                    _running = true;
                    _writer = new WaveFileWriter(_wav, AudioService.Format);
                    AudioService.Service.DataAvailable += Service_DataAvailable;
                    OnRecorderStatusChanged?.Invoke(this, _running, StarTime, new TimeSpan(0, 0, 0));
                }
            });
            t.Start();
        }
        public void Stop()
        {
            lock (_locker)
            {
                if (!_running) return;
                _running = false;
                AudioService.Service.DataAvailable -= Service_DataAvailable;
                _writer?.Dispose();
                _writer = null;
                Console.WriteLine("录音停止");
            }
            OnRecorderStatusChanged?.Invoke(this, Running, _starTime, TimeSpan.Zero);
        }
        private void Service_DataAvailable(object sender, byte[] e)
        {
            try
            {
                if (_writer == null || !Running) return;                
                _writer.Write(e, 0, e.Length);
                if (!((DateTime.Now - _lastAudioData).TotalMilliseconds > 100)) return;
                _lastAudioData = DateTime.Now;
                OnRecorderStatusChanged?.Invoke(this, Running, StarTime, _writer.TotalTime);
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp);
            }
        }
    }
}
