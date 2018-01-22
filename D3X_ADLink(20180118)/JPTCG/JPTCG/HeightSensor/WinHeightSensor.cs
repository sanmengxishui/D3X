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

namespace JPTCG.HeightSensor
{
    public partial class WinHeightSensor : Form
    {
        HeightSensorMgr myhMgr;
        public WinHeightSensor(HeightSensorMgr mgr)
        {
            InitializeComponent();
            myhMgr = mgr;
        }

        private void WinHeightSensor_Load(object sender, EventArgs e)
        {
            ModLB.Items.Clear();
            for (int i = 0; i < myhMgr.heightSensorList.Count; i++)
                ModLB.Items.Add(myhMgr.heightSensorList[i].Name);
            if (ModLB.Items.Count != 0)
            {
                ModLB.SelectedIndex = 0;
                UpdateUI(0);
            }
        }

        private void UpdateUI(int HgtIdx)
        {           
            cbxPort.Text = myhMgr.heightSensorList[HgtIdx].MyPortName.ToString();
            textBox1.Text = myhMgr.heightSensorList[HgtIdx].OriginValue.ToString();
           
        }

        private void AddMsg(string msg)
        {
            if (msgLB.InvokeRequired)
            {
                msgLB.Invoke(
                    new Action(() =>
                    {
                        msgLB.Items.Add(msg);
                    })
                    );
            }
            else
            {
                msgLB.Items.Add(msg);
            }
        }

        private void WinHeightSensor_FormClosing(object sender, FormClosingEventArgs e)
        {
            myhMgr.SaveSettings(Para.MchConfigFileName);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (ModLB.Items.Count == 0)
                return;

            int HgtIdx = ModLB.SelectedIndex;

            //myhMgr.heightSensorList[HgtIdx].MyPortName = ip1TB.Text;
            myhMgr.heightSensorList[HgtIdx].MyPortName = cbxPort.Text;
            if (!myhMgr.heightSensorList[HgtIdx].Connect(myhMgr.heightSensorList[HgtIdx].MyPortName))
                msgLB.Items.Add(myhMgr.heightSensorList[HgtIdx].Name + " Failed to connect.");
            else
                msgLB.Items.Add(myhMgr.heightSensorList[HgtIdx].Name + " Connected.");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (ModLB.Items.Count == 0)
                return;

            int HgtIdx = ModLB.SelectedIndex;

            myhMgr.heightSensorList[HgtIdx].Disconnect();
            msgLB.Items.Add(myhMgr.heightSensorList[HgtIdx].Name + " Disconected.");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (ModLB.Items.Count == 0)
                return;

            int HgtIdx = ModLB.SelectedIndex;

            if (!myhMgr.heightSensorList[HgtIdx].IsConnected())
                msgLB.Items.Add(myhMgr.heightSensorList[HgtIdx].Name + " Is Not Connected.");
            msgLB.Items.Add(myhMgr.heightSensorList[HgtIdx].Name + ">>" + myhMgr.heightSensorList[HgtIdx].Read().ToString());
            //msgLB.Items.Insert(0, myhMgr.heightSensorList[HgtIdx].Name + ">>" + myhMgr.heightSensorList[HgtIdx].Read().ToString());
        }

        private void ModLB_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateUI(ModLB.SelectedIndex);
        }

        private void msgLB_SelectedIndexChanged(object sender, EventArgs e)
        {
            Clipboard.SetDataObject(msgLB.Text);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //for (int i = 0; i < myhMgr.heightSensorList.Count; i++)
            //{
            //    myhMgr.heightSensorList[i].OriginValue = double.Parse(textBox1.Text);
            //    myhMgr.heightSensorList[i].SaveSettings(Para.MchConfigFileName);
            //}
            if (ModLB.SelectedIndex < 0)
                return;
            myhMgr.heightSensorList[ModLB.SelectedIndex].OriginValue = double.Parse(textBox1.Text);
            myhMgr.heightSensorList[ModLB.SelectedIndex].SaveSettings(Para.MchConfigFileName);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (ModLB.Items.Count == 0)
                return;

            int HgtIdx = ModLB.SelectedIndex;

            if (!myhMgr.heightSensorList[HgtIdx].IsConnected())
                msgLB.Items.Add(myhMgr.heightSensorList[HgtIdx].Name + " Is Not Connected.");

            myhMgr.heightSensorList[HgtIdx].SetZero();
            msgLB.Items.Add(myhMgr.heightSensorList[HgtIdx].Name + "Set zero success!" );
        }


    }
}
