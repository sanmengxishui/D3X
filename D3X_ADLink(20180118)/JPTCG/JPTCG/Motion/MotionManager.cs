using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PCI_DMC;
using PCI_DMC_ERR;
using System.Windows.Forms;
using System.Threading;
using JPTCG.Motion.ADLink;
using JPTCG.Common;
using Common;
using JPTCG.Motion;

namespace JPTCG.Motion
{
    public class MotionManager//20171216
    {
        private static bool firstInstance = false;
        private MotionType pMotionType = MotionType.Adlink;

        bool isOnline = false;

        public List<MotionPara> AxisPara = new List<MotionPara>();
        string motionSettingsFilePath = "";
        string motionSetFileName = "motion_Settings.xml";
        int HomingTimeoutMS = 300000;
        int MotionTimeoutMS = 180000;
        public List<string> AxisListDesc = new List<string>();
        public List<int> AxisListIndex = new List<int>();
        String exePath;

        private APS204CMotionMgr adlinkMgr;
        private DeltaMotionMgr deltaMgr;

        public MotionManager(MotionType myMotionType, bool IsMchOnline, Type myAxisList)
        {
            if (firstInstance)
            {
                MessageBox.Show("Error. Motion Manager already Created.");
                return;
            }

            isOnline = IsMchOnline;
            exePath = System.AppDomain.CurrentDomain.BaseDirectory;

            pMotionType = myMotionType;
            firstInstance = true;
            AxisListDesc = Helper.GetAllEnumDescription(myAxisList);
            AxisListIndex = Helper.GetAllEnumIntValue(myAxisList);
            CreateDefaultParameters();

            Init();

            motionSettingsFilePath = exePath + motionSetFileName;
            LoadMotionSettings();
        }

        public MotionManager()
        {
            SaveMotionSettings();
            CloseMotion();
        }

        private int Init()
        {
            if (!isOnline)
                return (int)Motion_ErrorList.NoError;

            int res = (int)Motion_ErrorList.CardInitFail;

            switch (pMotionType)
            {
                case MotionType.Delta:
                    deltaMgr = DeltaMotionMgr.CreateMotion(isOnline);
                    return deltaMgr.Init();
                case MotionType.Adlink:
                    adlinkMgr = new APS204CMotionMgr();
                    string AdlinkParaFileName = exePath ;
                    if (adlinkMgr.InitCard(AdlinkParaFileName))
                        return (int)Motion_ErrorList.NoError;
                    break;
            }

            return res;
        }

        private void ServoOffAllAxis()
        {
            for (int i = 0; i < AxisListIndex.Count; i++)
                SetServoON(AxisListIndex[i], false);
        }

        private void CloseMotion()
        {
            if (!isOnline)
                return;

            ServoOffAllAxis();
            switch (pMotionType)
            {
                case MotionType.Delta:
                    deltaMgr.CloseCard();
                    break;
                case MotionType.Adlink:
                    adlinkMgr.CloseCard();
                    break;
            }
        }

        public void LoadMotionSettings()
        {
            string strread = "";
            string headerStr = "";
            for (int i = 0; i < AxisPara.Count; i++)
            {
                headerStr = "Axis" + (i + 1).ToString();
             
                FileOperation.ReadData(motionSettingsFilePath, headerStr, "HomeSpeed", ref strread);
                if (strread != "0")
                    AxisPara[i].HomeSpeed = int.Parse(strread);

                FileOperation.ReadData(motionSettingsFilePath, headerStr, "RunSpeed", ref strread);
                if (strread != "0")
                    AxisPara[i].RunSpeed = int.Parse(strread);

                FileOperation.ReadData(motionSettingsFilePath, headerStr, "MotorScale", ref strread);
                if (strread != "0")
                    AxisPara[i].MotorScale = int.Parse(strread);

                FileOperation.ReadData(motionSettingsFilePath, headerStr, "IsServoMotor", ref strread);
                if (strread != "0")
                    AxisPara[i].IsServoMotor = bool.Parse(strread);
            }
        }

        public void SaveMotionSettings()
        {
            string headerStr = "";
            for (int i = 0; i < AxisPara.Count; i++)
            {
                headerStr = "Axis" + (i + 1).ToString();
                FileOperation.SaveData(motionSettingsFilePath, headerStr, "HomeSpeed", AxisPara[i].HomeSpeed.ToString());
                FileOperation.SaveData(motionSettingsFilePath, headerStr, "RunSpeed", AxisPara[i].RunSpeed.ToString());
                FileOperation.SaveData(motionSettingsFilePath, headerStr, "MotorScale", AxisPara[i].MotorScale.ToString());
                FileOperation.SaveData(motionSettingsFilePath, headerStr, "IsServoMotor", AxisPara[i].IsServoMotor.ToString());
            }
        }

        public MotionPara GetSettings(int AxisId)
        {
            for (int i = 0; i < AxisPara.Count; i++)
            {
                if (AxisPara[i].AxisIdx == AxisId)
                    return AxisPara[i];
            }
            return null;
        }

        public void CreateDefaultParameters()
        {
            AxisPara.Clear(); //Alvin 310817
            for (int i = 0; i < AxisListIndex.Count; i++)
            {
                MotionPara myPara = new MotionPara();
                myPara.AxisIdx = AxisListIndex[i];
                AxisPara.Add(myPara);
            }
        }

        public void ShowSettings()
        {
            WinMotionSettings myWin = new WinMotionSettings(this);
            myWin.ShowDialog();
        }

        //Motion Command
        public void SetServoON(int AxisID, bool val)
        {
            if (!isOnline)
                return;

            switch (pMotionType)
            {
                case MotionType.Delta:
                    if (val)
                        deltaMgr.SetServoOn((ushort)AxisID, 1);
                    else
                        deltaMgr.SetServoOn((ushort)AxisID, 0);
                    break;
                case MotionType.Adlink:
                    adlinkMgr.SetServoOn((int)AxisID, val);
                    break;
            }
        }

        public void Homing(int AxisID, int Mode)
        {
            if (!isOnline)
                return;

            GetSettings(AxisID).OnPreMove();

            switch (pMotionType)
            {
                case MotionType.Delta:
                    //deltaMgr.Homing((ushort)AxisID, (ushort)Mode, (ushort)GetSettings(AxisID).HomeSpeed, GetSettings(AxisID).IsServoMotor);
                    deltaMgr.Homing((ushort)AxisID, (ushort)Mode);
                    break;
                case MotionType.Adlink:
                    adlinkMgr.Homing((int)AxisID, Mode, GetSettings(AxisID).HomeSpeed);
                    break;
            }
        }

        public int WaitHomeDone(int AxisID)
        {
            if (!isOnline)
                return (int)Motion_ErrorList.NoError;

            switch (pMotionType)
            {
                case MotionType.Delta:
                    return deltaMgr.WaitHomeDone((ushort)AxisID);
                case MotionType.Adlink:
                    return adlinkMgr.WaitHomeDone((int)AxisID, HomingTimeoutMS);
            }
            return (int)Motion_ErrorList.NoError;
        }

        public int WaitAxisStop(int AxisID)
        {
            if (!isOnline)
                return (int)Motion_ErrorList.NoError;

            switch (pMotionType)
            {
                case MotionType.Delta:
                    return deltaMgr.WaitAxisStop((ushort)AxisID);
                case MotionType.Adlink:
                    return adlinkMgr.WaitAxisStop((int)AxisID, MotionTimeoutMS);
            }
            return (int)Motion_ErrorList.NoError;
        }

        public int MoveTo(int AxisID, double Position)
        {
            if (!isOnline)
                return (int)Motion_ErrorList.NoError;

            GetSettings(AxisID).OnPreMove();

            switch (pMotionType)
            {
                case MotionType.Delta:
                    return deltaMgr.MoveTo((ushort)AxisID, Position);
                case MotionType.Adlink:
                    return adlinkMgr.MoveTo((int)AxisID, Position, GetSettings(AxisID));
            }
            return (int)Motion_ErrorList.NoError;
        }

        public int StepMove(int AxisID, double Distance)
        {
            if (!isOnline)
                return (int)Motion_ErrorList.NoError;

            GetSettings(AxisID).OnPreMove();

            switch (pMotionType)
            {
                case MotionType.Delta:
                    return deltaMgr.StepMove((ushort)AxisID, Distance);
                case MotionType.Adlink:
                    return adlinkMgr.StepMove((int)AxisID, Distance, GetSettings(AxisID));
            }
            return (int)Motion_ErrorList.NoError;
        }

        public int StopMotion(int AxisID)
        {
            if (!isOnline)
                return (int)Motion_ErrorList.NoError;

            switch (pMotionType)
            {
                case MotionType.Delta:
                    deltaMgr.StopMotion((ushort)AxisID, 1);//20171218
                    break;
                case MotionType.Adlink:
                    adlinkMgr.StopMotion((int)AxisID);
                    break;
            }
            return (int)Motion_ErrorList.NoError;
        }

        public double GetPos(int AxisID)
        {
            if (!isOnline)
                return (int)Motion_ErrorList.NoError;

            switch (pMotionType)
            {
                case MotionType.Delta:
                    return deltaMgr.GetPos((ushort)AxisID);
                case MotionType.Adlink:
                    return adlinkMgr.GetPos((int)AxisID, GetSettings(AxisID));
            }
            return (int)Motion_ErrorList.NoError;
        }

        public void SetPosition(int AxisID, int Position)
        {
            if (!isOnline)
                return;

            switch (pMotionType)
            {
                case MotionType.Delta:
                    deltaMgr.SetPosition((ushort)AxisID, Position);
                    break;
                case MotionType.Adlink:
                    adlinkMgr.SetPosition((int)AxisID, Position);
                    break;
            }
        }

        public bool IsBusy(int AxisID)
        {
            if (!isOnline)
                return false;

            switch (pMotionType)
            {
                case MotionType.Delta:
                    //return deltaMgr.IsBusy((ushort)AxisID);
                case MotionType.Adlink:
                    return adlinkMgr.IsBusy((int)AxisID);
            }
            return false;
        }

        public bool NEL(int AxisID)
        {
            if (!isOnline)
                return false;

            switch (pMotionType)
            {
                case MotionType.Delta:
                    return deltaMgr.NEL((ushort)AxisID);
                case MotionType.Adlink:
                    return adlinkMgr.NEL((int)AxisID);
            }
            return false;
        }

        public bool PEL(int AxisID)
        {
            if (!isOnline)
                return false;

            switch (pMotionType)
            {
                case MotionType.Delta:
                    return deltaMgr.PEL((ushort)AxisID);
                case MotionType.Adlink:
                    return adlinkMgr.PEL((int)AxisID);
            }
            return false;
        }

        public bool ORG(int AxisID)
        {
            if (!isOnline)
                return false;

            switch (pMotionType)
            {
                case MotionType.Delta:
                    return deltaMgr.Org((ushort)AxisID);
                case MotionType.Adlink:
                    return adlinkMgr.Org((int)AxisID);
            }
            return false;
        }

        public bool ALM(int AxisID)
        {           
            if (!isOnline)
                return false;

            switch (pMotionType)
            {
                case MotionType.Delta:
                    uint alm_flag = deltaMgr.GetAlarmStatus((ushort)AxisID);//20171218
                    if (alm_flag == 0)
                        return true;
                    else
                        return false;
                case MotionType.Adlink:
                    return adlinkMgr.Alm((int)AxisID);
            }
            return false;
        }

        public void WriteIOOut(int CardIdx, int port, bool sts)
        {
            if (!isOnline)
                return;

            switch (pMotionType)
            {
                case MotionType.Delta:
                    deltaMgr.WriteIOOut((ushort)port, sts);
                    break;
                case MotionType.Adlink:
                    adlinkMgr.WriteIOOut(CardIdx, port, sts);
                    break;
            }
            return;
        }

        public bool ReadIOOut(int CardIdx, int port)
        {
            if (!isOnline)
                return false;

            switch (pMotionType)
            {
                case MotionType.Delta:
                    return deltaMgr.ReadIOOut((ushort)port);
                case MotionType.Adlink:
                    return adlinkMgr.ReadIOOut(CardIdx, port);
            }
            return false;
        }

        public bool ReadIOIn(int CardIdx, int port)
        {
            if (!isOnline)
                return false;

            switch (pMotionType)
            {
                case MotionType.Delta:
                    return deltaMgr.ReadIOIn((ushort)port);
                case MotionType.Adlink:
                    return adlinkMgr.ReadIOIn(CardIdx, port);
            }
            return false;
        }

        //Extra Functions for Delta Only
        public void ResetIO()
        {
            if (!isOnline)
                return;

            switch (pMotionType)
            {
                case MotionType.Delta:
                    deltaMgr.ResetIO();
                    break;
                case MotionType.Adlink:
                    adlinkMgr.resetAllIO();
                    break;
            }
            return;
        }

        public int ResetAlarm(int AxisID)
        {
            if (!isOnline)
                return (int)Motion_ErrorList.NoError;

            switch (pMotionType)
            {
                case MotionType.Delta:
                    deltaMgr.ResetAlarm((ushort)AxisID);
                    break;
                case MotionType.Adlink:
                    break;
            }
            return (int)Motion_ErrorList.NoError;
        }

        public void InitRM32NT(int CardIdx) //AxisID=12 for 162
        {
            if (!isOnline)
                return;

            switch (pMotionType)
            {
                case MotionType.Delta:
                    deltaMgr.InitRM32NT();
                    break;
                case MotionType.Adlink:
                    break;
            }
            return;
        }

        public void ResetRM32NT(int CardIdx) //AxisID=12 for 162
        {
            if (!isOnline)
                return;

            switch (pMotionType)
            {
                case MotionType.Delta:
                    deltaMgr.ResetRM32NT();
                    break;
                case MotionType.Adlink:
                    break;
            }
            return;
        }

        public void WriteIOOutRM32(int CardIdx, int port, bool sts) //AxisID=12 for 162
        {
            if (!isOnline)
                return;

            switch (pMotionType)
            {
                case MotionType.Delta:
                    deltaMgr.WriteIOOutRM32((ushort)CardIdx, (ushort)port, sts);
                    break;
                case MotionType.Adlink:
                    break;
            }
            return;
        }

        public bool ReadIOOutRM32(int CardIdx, ushort port)
        {
            if (!isOnline)
                return false;

            switch (pMotionType)
            {
                case MotionType.Delta:
                    return deltaMgr.ReadIOOutRM32((ushort)CardIdx, port);
                case MotionType.Adlink:
                    break;
            }
            return false;
        }
    }
}
