using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JwRecorder
{
    public partial class FWareIns : Form
    {
        public FWareIns()
        {
            InitializeComponent();
        }

        public static int DeviceCount
        {
            get { return WaveIn.DeviceCount; }
        }

        public static string GetDeviceName(int index)
        {
            return WaveIn.GetCapabilities(index).ProductName;
        }

        public int Device => TDevices.SelectedIndex;

        private void FWareIns_Load(object sender, EventArgs e)
        {
            LoadDevices();
        }
        private void LoadDevices()
        {
            if(DeviceCount < 1)
            {
                MessageBox.Show("无可用的录音设备");
                this.Close();
                return;
            }
            for(int i=0;i<DeviceCount;i++)
            {
                TDevices.Items.Add(GetDeviceName(i));
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
