using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using JPTCG.Common;

namespace JPTCG.LightTiming
{
    public partial class WinLightTiming : Form
    {
        public WinLightTiming()
        {
            InitializeComponent();
        }

        private void WinLightTiming_Load(object sender, EventArgs e)
        {
            List<string> portList = Helper.GetAllComPortNumber();
            for (int i = 0; i < portList.Count; i++)
            {
                comboBox1.Items.Add(portList[i]);
                //comboBox2.Items.Add(portList[i]);
            }

            if (comboBox1.Items.Count > 0)
            {
                comboBox1.SelectedIndex = comboBox1.Items.IndexOf(Para.myMain.LDLS1.MyPortName);
                //comboBox2.SelectedIndex = comboBox2.Items.IndexOf(Para.myMain.DLRS2.MyPortName);
            }
            LSRunTime.Text = Para.rtDays.ToString() + " day" + Para.rtHours.ToString("F1") + " h";
            //textBox2.Text = Para.rtHours.ToString();
            //textBox3.Text = Para.rtMins.ToString();
            //textBox4.Text = Para.rtSeconds.ToString();
            listBox1.Items.Clear();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == -1)
                return;
            if (Para.myMain.LDLS1.Connect(comboBox1.Text))
            {
                listBox1.Items.Add("LightSource 1 Connected");
                //MessageBox.Show("Connected.");
            }
            else
            {
                listBox1.Items.Add("LightSource 1 Fail To Connect");
                //MessageBox.Show("Fail To Connect.");
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Para.myMain.LDLS1.Disconnect();
            button2.Enabled = true;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            int count = 0;
            if (!Para.myMain.LDLS1.IsConnected())
            {
                listBox1.Items.Add("LightSource 1 Not Connected");
                return;
            }

            count = Para.myMain.LDLS1.ReadLight();
            Para.rtSeconds = count;
            Para.rtDays = count / (24 * 60 * 60);
            Para.rtHours = (double)(count - Para.rtDays * 24 * 60 * 60) / 3600;
            Para.rtMins = (double)(count - Para.rtDays * 24 * 60 * 60 - Para.rtHours * 60 * 60) / 60;

            listBox1.Items.Insert(0, "LightSource 1 Countting : " + count.ToString() + "s");
            UpDateRunningTime();
        }

        public void UpDateRunningTime()
        {
            Action ac = new Action(() =>
            {
                LSRunTime.Text = Para.rtDays.ToString() + " day" + Para.rtHours.ToString("F1") + " h";
                //textBox2.Text = Para.rtHours.ToString();
                //textBox3.Text = Para.rtMins.ToString();
                //textBox4.Text = Para.rtSeconds.ToString();
            });
            LSRunTime.BeginInvoke(ac);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            int TimeDay = 0;
            double TimeHour = 0;
            double TimeMin = 0;
            Int32 SetTime = 0;
            if (Para.myMain.LDLS1.IsConnected())
            {
                if (int.TryParse(text_d.Text, out TimeDay) && double.TryParse(text_h.Text, out TimeHour))//&& double.TryParse(text_m.Text, out TimeMin))
                {
                    if (MessageBox.Show("Whether to modify the current running time of source ?", "LightSource", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == System.Windows.Forms.DialogResult.OK)
                    {
                        SetTime = TimeDay * 24 * 60 * 60 + (int)(TimeHour * 60 * 60);
                        Para.myMain.LDLS1.WriteCommand(SetTime);
                    }
                }
                else
                {
                    MessageBox.Show("The input cannot be empty!");
                }
            }
            else
            {
                MessageBox.Show("Fail to connected!");
            }
        }        

        private void button1_Click(object sender, EventArgs e)
        {
            Para.myMain.LDLS1.SaveSettings(Para.MchConfigFileName);
        }

        private void WinLightTiming_FormClosing(object sender, FormClosingEventArgs e)
        {
            Para.myMain.LDLS1.SaveSettings(Para.MchConfigFileName);
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }


    }
}
