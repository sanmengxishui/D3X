using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace JPTCG.Motion//20171216
{
    public class MotionPara
    {
        public int AxisIdx;
        public int HomeSpeed = 100;
        public int RunSpeed = 100;
        public int MotorScale = 128000;
        public bool IsServoMotor = true;

        public delegate void OnPreMoveFunc();
        public event OnPreMoveFunc OnPreMoveFunction;

        public void OnPreMove()
        {
            if (OnPreMoveFunction != null)
                OnPreMoveFunction();
        }
    }

    public enum MotionType
    {
        [DescriptionAttribute("Delta")]
        Delta = 0,
        [DescriptionAttribute("Adlink")]
        Adlink = 1,
    }

    //public enum JogDir
    //{
    //    LeftRight = 0,
    //    UpDown = 1,
    //    Theta = 2,
    //}

    public enum Motion_ErrorList
    {
        [DescriptionAttribute("No Error")]
        NoError = 0,
        [DescriptionAttribute("No Motion Card Found.")]
        NoCardFound = -1,
        [DescriptionAttribute("Can't Boot PCI_DMC Card.")]
        BootFail = -2,
        [DescriptionAttribute("Card Initial Failed.")]
        CardInitFail = -3,
        [DescriptionAttribute("Card Slave Not Found.")]
        SlaveIsEmpty = -4,
        [DescriptionAttribute("Card Reset Alarm Fail.")]
        ResetAlmFail = -5,
        [DescriptionAttribute("Card Homing Timeout.")]
        HomingTimeout = -6,
        [DescriptionAttribute("Card Motion Timeout.")]
        MotionTimeout = -7,
        [DescriptionAttribute("Card Motion Error.")]
        MotionError = -8,
        [DescriptionAttribute("Not Safe to Move.")]
        MotionSafetyError = -9,
    }
}
