using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPTCG.Motion
{
        public enum Axislist//20171216
        {
            [DescriptionAttribute("Module 1 X Axis")]
            Mod1XAxis = 7,//10,
            [DescriptionAttribute("Module 1 Y Axis")]
            Mod1YAxis = 5,//3,
            [DescriptionAttribute("Module 1 Z Axis")]
            Mod1ZAxis = 1,//11,
            [DescriptionAttribute("Module 2 X Axis")]
            Mod2XAxis = 6,//9,
            [DescriptionAttribute("Module 2 Y Axis")]
            Mod2YAxis = 4,//1,           
            [DescriptionAttribute("Module 2 Z Axis")]
            Mod2ZAxis = 0,//8,
        }

        public enum InputIOlist//20171216
        {
            [DescriptionAttribute("Input Button Left")]
            BtnLeft = 8,//0,
            [DescriptionAttribute("Input Button Right")]
            BtnRight = 9,//1,

            [DescriptionAttribute("Stop Button")]
            BtnStop = 10,//2,
            [DescriptionAttribute("Home Button")]
            BtnHome = 12,//3,
            [DescriptionAttribute("Emergency Button")]
            BtnEMO = 11,//4,
            [DescriptionAttribute("Safety Sensor")]
            SafetySensor = 13,//5,
            [DescriptionAttribute("Door Sensor")]
            DoorSensor = 14,//6,
            [DescriptionAttribute("Air Pressure")]
            AirPressure = 15,//7,

            //Rotary
            [DescriptionAttribute("Rotary Origin")]
            RotaryOrigin = 16,//8,
            [DescriptionAttribute("Rotary Height")]
            RotaryHeight = 16,//9,
            [DescriptionAttribute("Rotary Motion Complete")]
            RotaryMotionDone = 17,//10,
            [DescriptionAttribute("Rotary Error")]
            RotaryError = 18,//11,

            // 4 module input
            [DescriptionAttribute("Input Button LeftFor4")]
            BtnLeftFor4 = 19,//13,
            [DescriptionAttribute("Input Button RightFor4")]
            BtnRightFor4 = 20,//14,

            //[DescriptionAttribute("Rotary Index 1 Vacuum 1 Sensor")]
            //RI1Vac1=0,
            //[DescriptionAttribute("Rotary Index 1 Vacuum 2 Sensor")]
            //RI1Vac2 = 1,
            //[DescriptionAttribute("Rotary Index 2 Vacuum 1 Sensor")]
            //RI2Vac1 = 2,
            //[DescriptionAttribute("Rotary Index 2 Vacuum 2 Sensor")]
            //RI2Vac2 = 3,
            //[DescriptionAttribute("Rotary Index 3 Vacuum 1 Sensor")]
            //RI3Vac1 = 4,
            //[DescriptionAttribute("Rotary Index 3 Vacuum 2 Sensor")]
            //RI3Vac2 = 5,
            //[DescriptionAttribute("Rotary Index 4 Vacuum 1 Sensor")]
            //RI4Vac1 = 6,
            //[DescriptionAttribute("Rotary Index 4 Vacuum 2 Sensor")]
            //RI4Vac2 = 7,
        }

        public enum OutputIOlist//20171216
        {
            [DescriptionAttribute("Green Light")]
            LampGreen = 8,//0, //运行绿色指示灯
            [DescriptionAttribute("Amber Light")]
            LampAmber = 9,//1,//运行黄色指示灯 
            [DescriptionAttribute("Red Light")]
            LampRed = 10,//2,   //运行红色指示灯 
            [DescriptionAttribute("Buzzer")]
            Buzzer = 11,//3,       //蜂鸣器

            [DescriptionAttribute("Rotary Index 1 Vacuum 1")]
            RI1Vac1 = 12,//4,
            [DescriptionAttribute("Rotary Index 1 Vacuum 2")]
            RI1Vac2 = 13,//5,//21,
            [DescriptionAttribute("Rotary Index 2 Vacuum 1")]
            RI2Vac1 = 14,//6,//5,
            [DescriptionAttribute("Rotary Index 2 Vacuum 2")]
            RI2Vac2 = 15,//7,//22,
            [DescriptionAttribute("Rotary Index 3 Vacuum 1")]
            RI3Vac1 = 16,//8,//6,
            [DescriptionAttribute("Rotary Index 3 Vacuum 2")]
            RI3Vac2 = 17,//9,
            [DescriptionAttribute("Rotary Index 4 Vacuum 1")]
            RI4Vac1 = 18,//10,//7,
            [DescriptionAttribute("Rotary Index 4 Vacuum 2")]
            RI4Vac2 = 19,//11,
            [DescriptionAttribute("Camera Light 1")]
            Cam1Light = 20,//13,
            [DescriptionAttribute("Camera Light 2")]
            Cam2Light = 21,//14,
            [DescriptionAttribute("Camera Light 3")]
            Cam3Light = 19,//12,//14,
            [DescriptionAttribute("Camera Light 4")]
            Cam4Light = 15,
            [DescriptionAttribute("Spectrum Light Source")]
            SpectrumLS = 22,//16,
            [DescriptionAttribute("Rotary Enabled")]
            RotaryEnabled = 23,//18,

            [DescriptionAttribute("Rotary Index CW")]
            RotaryIndexCW = 8,//19,
            [DescriptionAttribute("Rotary Home")]
            RotaryHome = 9,//20,
            [DescriptionAttribute("Rotary Stop")]
            RotaryStop = 10,//21,
            [DescriptionAttribute("Rotary Index CCW")]
            RotaryIndexCCW = 11,//22,
            [DescriptionAttribute("Machine Light")]
            MchLight = 12,//23,
            
        }

        public enum JogDir
        {
            LeftRight = 0,
            UpDown = 1,
        }
}
