using JPTCG.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JPTCG
{
    public partial class KeyenceDLRS_Settings : Form
    {
        public KeyenceDLRS_Settings()
        {
            InitializeComponent();
        }

        private void KeyenceDLRS_Settings_Load(object sender, EventArgs e)
        {
            List<string> portList = Helper.GetAllComPortNumber();
            for (int i = 0; i < portList.Count; i++)
            {
                comboBox1.Items.Add(portList[i]);
                comboBox2.Items.Add(portList[i]);
            }

            if (comboBox1.Items.Count > 0)
            {
                comboBox1.SelectedIndex = comboBox1.Items.IndexOf(Para.myMain.DLRS1.MyPortName);
                comboBox2.SelectedIndex = comboBox2.Items.IndexOf(Para.myMain.DLRS2.MyPortName);
            }

            listBox1.Items.Clear();

            textBox1.Text = Para.myMain.DLRS1.OriginValue.ToString();
            textBox2.Text = Para.myMain.DLRS2.OriginValue.ToString();

            if (Para.myMain.DLRS1.IsConnected())
                button2.Enabled = false;

            if (Para.myMain.DLRS2.IsConnected())
                button3.Enabled = false;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Para.myMain.DLRS1.OriginValue = double.Parse(textBox1.Text);
            Para.myMain.DLRS2.OriginValue = double.Parse(textBox2.Text);
            Para.myMain.DLRS1.SaveSettings(Para.MchConfigFileName);
            Para.myMain.DLRS2.SaveSettings(Para.MchConfigFileName);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == -1)
                return;
            if (Para.myMain.DLRS1.Connect(comboBox1.Items[comboBox1.SelectedIndex].ToString()))
            {
                listBox1.Items.Add("Module 1 Connected");
                MessageBox.Show("Connected.");
            }
            else
            {
                listBox1.Items.Add("Module 1 Fail To Connect");
                MessageBox.Show("Fail To Connect.");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (comboBox2.SelectedIndex == -1)
                return;
            if (Para.myMain.DLRS2.Connect(comboBox2.Items[comboBox2.SelectedIndex].ToString()))
            {
                listBox1.Items.Add("Module 2 Connected");
                MessageBox.Show("Connected.");
            }
            else
            {
                listBox1.Items.Add("Module 2 Fail To Connect");
                MessageBox.Show("Fail To Connect.");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (!Para.myMain.DLRS1.IsConnected())
            {
                listBox1.Items.Add("Module 1 Not Connected");
                return;
            }

            listBox1.Items.Add("Module 1 Reading : "+Para.myMain.DLRS1.Read().ToString());
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (!Para.myMain.DLRS2.IsConnected())
            {
                listBox1.Items.Add("Module 2 Not Connected");
                return;
            }

            listBox1.Items.Add("Module 2 Reading : "+Para.myMain.DLRS2.Read().ToString());
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Para.myMain.DLRS1.Disconnect();
            button2.Enabled = true;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Para.myMain.DLRS2.Disconnect();
            button3.Enabled = true;
        }
    }
}
