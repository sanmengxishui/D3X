using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPTCG.HeightSensor
{
    public class HeightSensorMgr
    {
        public List<OptexCD33> heightSensorList = new List<OptexCD33>();

        public HeightSensorMgr()
        { }

        ~HeightSensorMgr()
        { }

        public void AddHeightsensor(string name)
        {
            OptexCD33 myHeight = new OptexCD33(name);
            heightSensorList.Add(myHeight);
        }

        public void LoadSettings(string FileName)
        {
            for (int i = 0; i < heightSensorList.Count; i++)
                heightSensorList[i].LoadSettings(FileName);
        }

        public void SaveSettings(string FileName)
        {
            for (int i = 0; i < heightSensorList.Count; i++)
                heightSensorList[i].SaveSettings(FileName);
        }

        public void ShowSettings()
        {
            //WinHeightSensor myHeightWin = new WinHeightSensor(this);
            //myHeightWin.ShowDialog();
        }
    }
}
