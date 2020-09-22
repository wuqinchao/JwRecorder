using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JwRecorder
{
    public partial class FRecorder : Form
    {
        private AudioRecorder _recorder;
        private bool _running = false;
        public FRecorder()
        {
            InitializeComponent();
        }

        private int _DeviceNum;

        private void FRecorder_Load(object sender, EventArgs e)
        {
            SetDevice();
            InitService();
            StartService();
        }

        private void SetDevice()
        {
            using (var f = new FWareIns())
            {
                if (f.ShowDialog() != DialogResult.OK)
                {
                    this.Close();
                    return;
                }

                _DeviceNum = f.Device;
            }
        }
        private void InitService()
        {
            AudioService.Service.ServiceStarted += Service_ServiceStarted;
            AudioService.Service.ServiceStopped += Service_ServiceStopped;            
        }

        private void Service_ServiceStopped(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void Service_ServiceStarted(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void StartService()
        {
            AudioService.Service.Start(_DeviceNum);
        }
        private void StopService()
        {
            AudioService.Service.Stop();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            BeginRecord();
            button1.Enabled = false;
            button2.Enabled = true;
            label1.Enabled = false;
        }
        private void BeginRecord()
        {
            if (_recorder == null)
            {
                _recorder = new AudioRecorder();
                label1.Text = _recorder.File;
                _recorder.OnRecorderStatusChanged += Recorder_OnRecorderStatusChanged;
                _recorder.Start();
            }
        }
        private void StopRecord()
        {
            if (_recorder != null)
            {
                _recorder.OnRecorderStatusChanged -= Recorder_OnRecorderStatusChanged;
                _recorder.Stop();
                _recorder = null;
                this.Invoke(new Action(() =>
                {
                    label2.Text = "录音结束";
                }));
            }
        }
        private void Recorder_OnRecorderStatusChanged(object sender, bool recording, DateTime starTime, TimeSpan totalTime)
        {
            this.Invoke(new Action(() =>
            {
                if (!recording)
                {
                    label2.Text = "录音结束";
                }
                else
                {
                    label2.Text = totalTime.ToString(@"hh\:mm\:ss");
                }
            }));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            StopRecord();
            button1.Enabled = true;
            button2.Enabled = false;
            label1.Enabled = true;
        }

        private void FRecorder_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopRecord();
            StopService();
        }

        private void label1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            AudioPlayer player = new AudioPlayer();
            player.LoadFile(label1.Text);
            player.Play();
        }
    }
}
