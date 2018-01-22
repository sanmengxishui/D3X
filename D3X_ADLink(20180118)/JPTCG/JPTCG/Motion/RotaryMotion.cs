using JPTCG.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JPTCG.Motion
{
    public class RotaryMotion
    {
        //DeltaMotionMgr myMgr;
        MotionManager myMgr;//20171216
        public bool InTestCameraDone;
        public RotaryMotion(MotionManager myMotion)
        {
            myMgr = myMotion;
            InTestCameraDone = false;
        }
        ~RotaryMotion()
        {           
        }

        public bool GoHome()
        {
            bool res = false;
            myMgr.WriteIOOut(1,(ushort)OutputIOlist.RotaryEnabled, false);
            Thread.Sleep(300);

            myMgr.WriteIOOut(1,(ushort)OutputIOlist.RotaryEnabled,true);
            Thread.Sleep(300);
            //bool val = myMgr.ReadIOOut((ushort)OutputIOlist.RotaryEnabled); //Not Enabled

            //if (!val)
            //    return res;

            if ((myMgr.GetPos((ushort)Axislist.Mod1YAxis) > 2) || (myMgr.GetPos((ushort)Axislist.Mod2YAxis) > 2))
                return res;

            bool val = myMgr.ReadIOIn(1,(ushort)InputIOlist.RotaryError); //Not Enabled

            if (!val)
                return res;
            if ((!myMgr.ReadIOIn(1,(ushort)InputIOlist.SafetySensor)) && (!myMgr.ReadIOIn(1,(ushort)InputIOlist.BtnStop)))
            {
                if (Para.DisableSafeDoor)
                {
                    myMgr.WriteIOOut(0,(ushort)OutputIOlist.RotaryHome, true);
                    Thread.Sleep(100);
                    myMgr.WriteIOOut(0,(ushort)OutputIOlist.RotaryHome, false);
                    Thread.Sleep(1000);
                }
                else 
                {
                    if (myMgr.ReadIOIn(1,(ushort)InputIOlist.DoorSensor))
                    {
                        
                        myMgr.WriteIOOut(0,(ushort)OutputIOlist.RotaryHome, true);
                        Thread.Sleep(100);
                        myMgr.WriteIOOut(0,(ushort)OutputIOlist.RotaryHome, false);
                        Thread.Sleep(1000);
                        
                    }
                    else  return false;
                }
            }
            else
            {
                //MessageBox.Show("SafeSenser Is Touched");
                return false;
            }

            DateTime st_time = DateTime.Now;
            TimeSpan time_span;
            while (!myMgr.ReadIOIn(1,(ushort)InputIOlist.RotaryOrigin))
            {
                Thread.Sleep(10);
                Application.DoEvents();

                if (!Para.DisableSafeDoor)
                {
                    if (!myMgr.ReadIOIn(1,(ushort)InputIOlist.DoorSensor))
                    {
                        myMgr.WriteIOOut(0,(ushort)OutputIOlist.RotaryStop, true);
                        //myMgr.WriteIOOut((ushort)OutputIOlist.RotaryEnabled, false);
                        Thread.Sleep(200);
                        myMgr.WriteIOOut(0,(ushort)OutputIOlist.RotaryStop, false);
                        Thread.Sleep(100);
                        myMgr.WriteIOOut(1,(ushort)OutputIOlist.RotaryEnabled, false);
                        return false;
                    }
                }


                if ((myMgr.ReadIOIn(1,(ushort)InputIOlist.SafetySensor)) || (myMgr.ReadIOIn(1,(ushort)InputIOlist.BtnStop)))
                {
                    //if (Para.DisableSafeDoor)
                    //{
                        myMgr.WriteIOOut(0,(ushort)OutputIOlist.RotaryStop, true);
                        //myMgr.WriteIOOut((ushort)OutputIOlist.RotaryEnabled, false);
                        Thread.Sleep(200);
                        myMgr.WriteIOOut(0,(ushort)OutputIOlist.RotaryStop, false);
                        Thread.Sleep(100);
                        myMgr.WriteIOOut(1,(ushort)OutputIOlist.RotaryEnabled, false);
                        return false;
                    //}
                    //else 
                    //{
                    //    if (!myMgr.ReadIOIn((ushort)InputIOlist.DoorSensor))
                    //    {
                    //        myMgr.WriteIOOut((ushort)OutputIOlist.RotaryStop, true);
                    //        //myMgr.WriteIOOut((ushort)OutputIOlist.RotaryEnabled, false);
                    //        Thread.Sleep(200);
                    //        myMgr.WriteIOOut((ushort)OutputIOlist.RotaryStop, false);
                    //        Thread.Sleep(100);
                    //        myMgr.WriteIOOut((ushort)OutputIOlist.RotaryEnabled, false);
                    //        return false;
                    //    }
                    //}
                }

                time_span = DateTime.Now - st_time;
                if (time_span.TotalMilliseconds > 10000)
                {
                    return false;
                }
            }
            res = true;
            Para.CurrentRotaryIndex = 0;
            Para.isRotaryAt45 = false;
            return res;
        }
        public bool IndexRotaryMotion()
        {
            myMgr.WriteIOOut(1,(ushort)OutputIOlist.RI1Vac1, true);
            myMgr.WriteIOOut(1,(ushort)OutputIOlist.RI1Vac2, true);

            myMgr.WriteIOOut(1,(ushort)OutputIOlist.RI2Vac1, true);
            myMgr.WriteIOOut(1,(ushort)OutputIOlist.RI2Vac2, true);

            myMgr.WriteIOOut(1,(ushort)OutputIOlist.RI3Vac1, true);
            myMgr.WriteIOOut(1,(ushort)OutputIOlist.RI3Vac2, true);

            myMgr.WriteIOOut(1,(ushort)OutputIOlist.RI4Vac1, true);
            myMgr.WriteIOOut(1,(ushort)OutputIOlist.RI4Vac2, true);
            Thread.Sleep(100);
                  
            bool res = false;
            bool val = myMgr.ReadIOOut(1,(ushort)OutputIOlist.RotaryEnabled); //Not Enabled

            if (!val)
                return res;

            if ((myMgr.GetPos((ushort)Axislist.Mod1YAxis) > 2) || (myMgr.GetPos((ushort)Axislist.Mod2YAxis) > 2))
                return res;

            val = myMgr.ReadIOIn(1,(ushort)InputIOlist.RotaryError); //Not Enabled

            if (!val)
                return res;

            if ((!myMgr.ReadIOIn(1,(ushort)InputIOlist.SafetySensor)) && (!myMgr.ReadIOIn(1,(ushort)InputIOlist.BtnStop)))
            {
                if (Para.DisableSafeDoor)
                {
                    myMgr.WriteIOOut(0,(ushort)OutputIOlist.RotaryIndexCW, true);
                    Thread.Sleep(100);
                    myMgr.WriteIOOut(0,(ushort)OutputIOlist.RotaryIndexCW, false);
                    Thread.Sleep(100);
                }
                else
                {
                    if (myMgr.ReadIOIn(1,(ushort)InputIOlist.DoorSensor))
                    {
                        myMgr.WriteIOOut(0,(ushort)OutputIOlist.RotaryIndexCW, true);
                        Thread.Sleep(100);
                        myMgr.WriteIOOut(0,(ushort)OutputIOlist.RotaryIndexCW, false);
                        Thread.Sleep(100);
                    }
                    else return false;
                }
            }
            else return false;
            //myMgr.WriteIOOut((ushort)OutputIOlist.RotaryIndexCW, true);
            //Thread.Sleep(100);
            //myMgr.WriteIOOut((ushort)OutputIOlist.RotaryIndexCW, false);
            //Thread.Sleep(100);


            InTestCameraDone = true;//20161214

            DateTime st_time = DateTime.Now;
            TimeSpan time_span;
            while (!myMgr.ReadIOIn(1,(ushort)InputIOlist.RotaryMotionDone))
            {
                Thread.Sleep(10);
                Application.DoEvents();
                time_span = DateTime.Now - st_time;

                if (!Para.DisableSafeDoor)
                {
                    if (!myMgr.ReadIOIn(1,(ushort)InputIOlist.DoorSensor))
                    {
                        myMgr.WriteIOOut(0,(ushort)OutputIOlist.RotaryStop, true);
                        //myMgr.WriteIOOut((ushort)OutputIOlist.RotaryEnabled, false);
                        Thread.Sleep(200);
                        myMgr.WriteIOOut(1,(ushort)OutputIOlist.RotaryStop, false);
                        Thread.Sleep(100);
                        myMgr.WriteIOOut(1,(ushort)OutputIOlist.RotaryEnabled, false);
                        return false;
                    }
                }


                if ((myMgr.ReadIOIn(1,(ushort)InputIOlist.SafetySensor)) || (myMgr.ReadIOIn(1,(ushort)InputIOlist.BtnStop)))
                {
                    //if (Para.DisableSafeDoor)
                    //{
                    myMgr.WriteIOOut(0,(ushort)OutputIOlist.RotaryStop, true);
                    //myMgr.WriteIOOut((ushort)OutputIOlist.RotaryEnabled, false);
                    Thread.Sleep(200);
                    myMgr.WriteIOOut(0,(ushort)OutputIOlist.RotaryStop, false);
                    Thread.Sleep(100);
                    myMgr.WriteIOOut(1,(ushort)OutputIOlist.RotaryEnabled, false);
                    return false;
                }
                if (time_span.TotalMilliseconds > 20000)
                {
                    return false;
                }
            }
            Thread.Sleep(100);//from 100 t00 500 //20161101
            res = true;
            if (Para.Enb45DegTest)
            {
                if (Para.EnableP2Test)
                {
                    if (!Para.isRotaryAt45)
                    {
                        Para.isRotaryAt45 = true;
                        return res;
                    }
                    else
                    {
                        Para.isRotaryAt45 = false;
                        int curIdx = Para.CurrentRotaryIndex + 1;
                        if (curIdx >= 4)
                            Para.CurrentRotaryIndex = 0;
                        else
                            Para.CurrentRotaryIndex = curIdx;
                    }
                }
                else
                {
                    int curIdx = Para.CurrentRotaryIndex + 1;
                    if (curIdx >= 4)
                        Para.CurrentRotaryIndex = 0;
                    else
                        Para.CurrentRotaryIndex = curIdx;
                }
            }
            else
            {
                int curIdx = Para.CurrentRotaryIndex + 1;
                if (curIdx >= 4)
                    Para.CurrentRotaryIndex = 0;
                else
                    Para.CurrentRotaryIndex = curIdx;
            }
            return res;
        }

        public bool IndexRotaryMotionCCW()
        {
            myMgr.WriteIOOut(1,(ushort)OutputIOlist.RI1Vac1, true);
            myMgr.WriteIOOut(1,(ushort)OutputIOlist.RI1Vac2, true);

            myMgr.WriteIOOut(1,(ushort)OutputIOlist.RI2Vac1, true);
            myMgr.WriteIOOut(1,(ushort)OutputIOlist.RI2Vac2, true);

            myMgr.WriteIOOut(1,(ushort)OutputIOlist.RI3Vac1, true);
            myMgr.WriteIOOut(1,(ushort)OutputIOlist.RI3Vac2, true);

            myMgr.WriteIOOut(1,(ushort)OutputIOlist.RI4Vac1, true);
            myMgr.WriteIOOut(1,(ushort)OutputIOlist.RI4Vac2, true);
            Thread.Sleep(100);

            bool res = false;
            bool val = myMgr.ReadIOOut(1,(ushort)OutputIOlist.RotaryEnabled); //Not Enabled

            if (!val)
                return res;

            val = myMgr.ReadIOIn(1,(ushort)InputIOlist.RotaryError); //Not Enabled

            if (!val)
                return res;

            if ((myMgr.GetPos((ushort)Axislist.Mod1YAxis) > 2) || (myMgr.GetPos((ushort)Axislist.Mod2YAxis) > 2))
                return res;

            myMgr.WriteIOOut(0,(ushort)OutputIOlist.RotaryIndexCCW, true);
            Thread.Sleep(100);
            myMgr.WriteIOOut(0,(ushort)OutputIOlist.RotaryIndexCCW, false);
            Thread.Sleep(100);

            DateTime st_time = DateTime.Now;
            TimeSpan time_span;
            while (!myMgr.ReadIOIn(1,(ushort)InputIOlist.RotaryMotionDone))
            {
                Thread.Sleep(10);
                Application.DoEvents();
                time_span = DateTime.Now - st_time;
                if (myMgr.ReadIOIn(1,(ushort)InputIOlist.SafetySensor))
                {
                    myMgr.WriteIOOut(0,(ushort)OutputIOlist.RotaryStop, true);
                    //myMgr.WriteIOOut((ushort)OutputIOlist.RotaryEnabled, false);
                    Thread.Sleep(200);
                    myMgr.WriteIOOut(0,(ushort)OutputIOlist.RotaryStop, false);
                    Thread.Sleep(100);
                    myMgr.WriteIOOut(1,(ushort)OutputIOlist.RotaryEnabled, false);
                    return false;
                }
                if (time_span.TotalMilliseconds > 1000)
                {
                    return false;
                }
            }
            Thread.Sleep(200);
            res = true;
            int curIdx = Para.CurrentRotaryIndex - 1;
            if (curIdx < 0)
                Para.CurrentRotaryIndex = 3;
            else
                Para.CurrentRotaryIndex = curIdx;

            return res;
        }
    }
}
