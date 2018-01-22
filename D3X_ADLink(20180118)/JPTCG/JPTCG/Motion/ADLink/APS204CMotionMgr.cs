using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using APS168_W32;
using APS_Define_W32;
using System.Threading;
using System.Collections;

namespace JPTCG.Motion.ADLink
{
   public class APS204CMotionMgr
    {
        //******************************Main data*****************************************//
        const Int32 YES = 1;
        const Int32 NO = 0;
        const Int32 ON = 1;
        const Int32 OFF = 0;

        Int32 v_card_name = 0;
        Int32 v_board_id = -1;
        Int32 v_channel = 0;
        Int32 v_total_axis = 0;
        Int32 v_is_card_initialed = 0;
        //////////////////////////////////////////////////////////////////////////////////
        Int32 Is_Creat = NO;
        protected Object m_LockObject;
        private static APS204CMotionMgr h_motion;

        public static APS204CMotionMgr CreateMotion()
        {
            if (h_motion == null) h_motion = new APS204CMotionMgr();
            return h_motion;
        }

        public APS204CMotionMgr()
        {
            m_LockObject = new Object();
        }

        public bool InitCard(string ParaFileName)
        {
            try
            {
                Int32 boardID_InBits = 0;
                Int32 mode = 0;
                Int32 ret = 0;
                Int32 i = 0;
                Int32 card_name = 0;
                Int32 tamp = 0;
                Int32 StartAxisID = 0;
                Int32 TotalAxisNum = 0;

                // UI protection
                //if (v_is_card_initialed == YES)
                //{
                //    MessageBox.Show("Initialize successfully !!");
                //    return;
                //}
                // Card(Board) initial
                ret = APS168.APS_initial(ref boardID_InBits, mode);

                if (ret == 0)
                {
                    for (i = 0; i < 16; i++)
                    {
                        tamp = (boardID_InBits >> i) & 1;

                        if (tamp == 1)
                        {
                            ret = APS168.APS_get_card_name(i, ref card_name);

                            if (card_name == (Int32)APS_Define.DEVICE_NAME_PCI_825458
                                || card_name == (Int32)APS_Define.DEVICE_NAME_AMP_20408C)
                            {
                                ret = APS168.APS_get_first_axisId(i, ref  StartAxisID, ref  TotalAxisNum);

                                //----------------------------------------------------
                                v_card_name = card_name;
                                v_board_id = i;
                                v_total_axis = TotalAxisNum;
                                v_is_card_initialed = YES;

                                if (v_total_axis == 4) v_channel = 2;
                                else if (v_total_axis == 8) v_channel = 4;
                                //----------------------------------------------------
                                Is_Creat = NO;
                                //string configFilename = ParaFileName;//@"D:\paramA167.XML";
                                //string configFilename = @"D:\param-A162-card" + i.ToString()+".XML";
                                string AdlinkParaFileName = ParaFileName + "param-A162-card" +i+ ".XML";
                                int res = APS168.APS_load_param_from_file(AdlinkParaFileName);
                                if (res != 0)
                                    return false;                            
                            }
                        }
                    }                   
                    return true;
                    if (v_board_id == -1)
                    {
                        v_is_card_initialed = NO;
                        MessageBox.Show("Board Id search error !");
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    v_is_card_initialed = NO;
                    MessageBox.Show("Initialize fail");
                    return false;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Exception From InitCard !");
                return false;
            }
        }

        public  Int32 check_motion_done_example(Int32 Axis_ID, ref Int32 Stop_Code)
        {
            Int32 axis_id = Axis_ID;
            Int32 msts = 0;
            Int32 m_stop_code = 0;

            msts = APS168.APS_motion_status(axis_id); // Get motion status
            msts = (msts >> 5) & 1;                   // Get motion done bit

            // Get stop code.
            APS168.APS_get_stop_code(Axis_ID, ref Stop_Code);

            if (msts == 1)
            {
                // Check move success or not
                msts = APS168.APS_motion_status(axis_id); // Get motion status
                msts = (msts >> 16) & 1;                  // Get abnormal stop bit

                if (msts == 1)
                {   // Error handling ...
                    APS168.APS_get_stop_code(axis_id, ref m_stop_code);
                    return -1; //error
                }
                else
                {   // Motion success.
                    return 1;
                }
            }

            return 0; //Now are in motion
        }

        public void CloseCard()
        {
            APS168.APS_close();
        }

        public void resetAllIO()
        {
            APS168.APS_write_d_output(0, 0, 0);
            Thread.Sleep(100);
            APS168.APS_write_d_output(1, 0, 0);
        }

        //Motion　Command
        public void SetServoOn(int AxisID, bool val)
        {
            if (val)
                APS168.APS_set_servo_on(AxisID, 1);
            else
                APS168.APS_set_servo_on(AxisID, 0);
        }

        public  void home_move(Int32 Axis_ID)
        {
            //This example shows how home move operation
            Int32 axis_id = Axis_ID;
            Int32 return_code = 0;

            // 1. Select home mode and config home parameters 
            APS168.APS_set_axis_param(axis_id, (Int32)APS_Define.PRA_HOME_MODE, 0); // Set home mode
            APS168.APS_set_axis_param(axis_id, (Int32)APS_Define.PRA_HOME_DIR, 1); // Set home direction
            APS168.APS_set_axis_param_f(axis_id, (Int32)APS_Define.PRA_HOME_CURVE, 0); // Set acceleration paten (T-curve)
            APS168.APS_set_axis_param_f(axis_id, (Int32)APS_Define.PRA_HOME_ACC, 1000000); // Set homing acceleration rate
            APS168.APS_set_axis_param_f(axis_id, (Int32)APS_Define.PRA_HOME_VM, 100000); // Set homing maximum velocity.
            APS168.APS_set_axis_param_f(axis_id, (Int32)APS_Define.PRA_HOME_VO, 50000); // Set homing VO speed
            APS168.APS_set_axis_param(axis_id, (Int32)APS_Define.PRA_HOME_EZA, 0); // Set EZ signal alignment (yes or no)
            APS168.APS_set_axis_param_f(axis_id, (Int32)APS_Define.PRA_HOME_SHIFT, 0); // Set home position shfit distance. 
            APS168.APS_set_axis_param_f(axis_id, (Int32)APS_Define.PRA_HOME_POS, 0); // Set final home position.
            //servo on
            APS168.APS_set_servo_on(axis_id, 1);
            Thread.Sleep(500); // Wait stable.
            // 2. Start home move
            return_code = APS168.APS_home_move(axis_id); //Start homing 
            if (return_code != (Int32)APS_Define.ERR_NoError)
            { /* Error handling */ }
        }

        public void Homing(int AxisID, int Mode, int HomeSpeed) //Mode 0:ORG 1:Positive 2:Negative
        {
            //Home Move Parameter
            APS168.APS_set_axis_param(AxisID, (Int32)APS_Define.PRA_HOME_MODE, Mode);		  //Set PRA_HOME_MODE
            if (Mode == 2)
                APS168.APS_set_axis_param(AxisID, (Int32)APS_Define.PRA_HOME_DIR, 1);
            else
                APS168.APS_set_axis_param(AxisID, (Int32)APS_Define.PRA_HOME_DIR, 1);		  //Set PRA_HOME_DIR   0:Positive 1:Negative
            APS168.APS_set_axis_param(AxisID, (Int32)APS_Define.PRA_HOME_VM, HomeSpeed);	  //Set PRA_HOME_VM
            APS168.APS_set_axis_param(AxisID, (Int32)APS_Define.PRA_HOME_EZA, 0);		    //Set PRA_HOME_EZA 
            APS168.APS_set_axis_param(AxisID, (Int32)APS_Define.PRA_HOME_VO, HomeSpeed);		  //Set PRA_HOME_VO
            APS168.APS_set_axis_param(AxisID, (Int32)APS_Define.PRA_HOME_OFFSET, 50);	    //Set PRA_HOME_OFFSET 

            APS168.APS_home_move(AxisID);
            // APS_set_axis_param(Axis_ID, PRA_HOME_DIR, 1); //Set home direction APS_set_axis_param( Axis_ID, PRA_HOME_CURVE, 0 ); //Set acceleration paten (T-curve) APS_set_axis_param( Axis_ID, PRA_HOME_ACC, 1000000 ); //Set homing acceleration rate APS_set_axis_param( Axis_ID, PRA_HOME_VS, 0 ); //Set homing start velocity APS_set_axis_param( Axis_ID, PRA_HOME_VM, 2000000 ); //Set homing maximum velocity. APS_set_axis_param( Axis_ID, PRA_HOME_VO, 200000 ); //Set homing 
        }

        public int WaitHomeDone(int AxisID, int HomingTimeoutMS)
        {
            return WaitAxisStop(AxisID, HomingTimeoutMS);
        }

        public int WaitAxisStop(int AxisID, int MotionTimeoutMS)
        {
            DateTime st_time = DateTime.Now;
            TimeSpan time_span;
            while (IsBusy(AxisID))
            {
                time_span = DateTime.Now - st_time;
                if (time_span.TotalMilliseconds > MotionTimeoutMS)
                    return (int)Motion_ErrorList.MotionTimeout;

                Application.DoEvents();
                Thread.Sleep(10);
            }
            return (int)Motion_ErrorList.NoError;
        }

        public bool IsBusy(int AxisID)
        {
            int MotionSts = APS168.APS_motion_status(AxisID);
            if ((MotionSts & (0x01 << 0)) == 0)
            {
                return true;
            }
            if ((MotionSts & (0x01 << 6)) != 0)
            {
                return true;
            }

            return false;
        }

        public int MoveTo(int AxisID, double Position, MotionPara RunProfile)
        {
            //Single Move Parameter
            APS168.APS_set_axis_param(AxisID, (Int32)APS_Define.PRA_CURVE, 0);			                //Set PRA_CURVE  0:T-Curve 1:S-Curve
            APS168.APS_set_axis_param(AxisID, (Int32)APS_Define.PRA_ACC, RunProfile.RunSpeed * 10);		  //Set PRA_ACC 
            APS168.APS_set_axis_param(AxisID, (Int32)APS_Define.PRA_DEC, RunProfile.RunSpeed * 10);		  //Set PRA_DEC
            APS168.APS_set_axis_param(AxisID, (Int32)APS_Define.PRA_VS, 0);

            Int32 PositionInPulse = (Int32)(RunProfile.MotorScale * Position);

            if (APS168.APS_absolute_move(AxisID, PositionInPulse, RunProfile.RunSpeed) != 0)
                return (int)Motion_ErrorList.MotionError;

            return (int)Motion_ErrorList.NoError;
        }

        public int StepMove(int AxisID, double Dist, MotionPara RunProfile)
        {
            //Single Move Parameter
            APS168.APS_set_axis_param(AxisID, (Int32)APS_Define.PRA_CURVE, 0);			  //Set PRA_CURVE  0:T-Curve 1:S-Curve
            APS168.APS_set_axis_param(AxisID, (Int32)APS_Define.PRA_ACC, RunProfile.RunSpeed * 10);		  //Set PRA_ACC 
            APS168.APS_set_axis_param(AxisID, (Int32)APS_Define.PRA_DEC, RunProfile.RunSpeed * 10);		  //Set PRA_DEC
            APS168.APS_set_axis_param(AxisID, (Int32)APS_Define.PRA_VS, 0);

            Int32 DistInPulse = (Int32)(RunProfile.MotorScale * Dist);

            if (APS168.APS_relative_move(AxisID, DistInPulse, RunProfile.RunSpeed) != 0)
                return (int)Motion_ErrorList.MotionError;

            return (int)Motion_ErrorList.NoError;
        }

        public void StopMotion(int AxisID)
        {
            APS168.APS_emg_stop(AxisID);
        }

        public double GetPos(int AxisID, MotionPara AxisProfile)
        {
            Int32 Position = 0;
            //APS168.APS_get_position(AxisID, ref Position);            
            APS168.APS_get_command(AxisID, ref Position);
            double codepos = 0.0;
            codepos = (double)Position / AxisProfile.MotorScale;

            return codepos;
        }

        public void SetPosition(int AxisID, int Pos)
        {
            APS168.APS_set_command(AxisID, Pos);
            //APS168.APS_set_position(AxisID, Pos);
        }

        public bool NEL(int AxisID)
        {
            return ReadServoDI(AxisID, 2);
        }

        public bool PEL(int AxisID)
        {
            return ReadServoDI(AxisID, 1);
        }

        public bool Org(int AxisID)
        {
            return ReadServoDI(AxisID, 3);
        }

        public bool Alm(int AxisID)
        {
            return ReadServoDI(AxisID, 0);
        }

        bool ReadServoDI(int AxisID, int BitNum)
        {
            lock (m_LockObject)
            {
                int MotionIOSts = APS168.APS_motion_io_status(AxisID);
                if ((MotionIOSts & (0x01 << BitNum)) != 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public void WriteIOOut(int ModIdx, int port, bool sts)
        {
            int IOStatus = 0;
            lock (m_LockObject)
            {
                //20171105
                //if (APS168.APS_get_field_bus_d_output(1, 0, ModIdx, ref IOStatus) != 0)
                //    return;
                if (APS168.APS_read_d_output(ModIdx, 0, ref IOStatus) != 0)
                    return;

                //APS168.APS_read_d_output(card, 0, ref IOStatus);
                BitArray myVal = new BitArray(new int[] { (int)IOStatus });
                myVal[port] = sts;
                Byte[] myB = new Byte[4];
                myVal.CopyTo(myB, 0);

                int IOOutStatus = BitConverter.ToInt32(myB, 0);
                //APS168.APS_write_d_output(card, 0, IOOutStatus);
                if (APS168.APS_write_d_output(ModIdx, 0, IOOutStatus) != 0)
                    return;
            }
        }

        public void WriteLightIOOut(int ModIdx, int port, bool sts)
        {
            int IOStatus = 0;
            lock (m_LockObject)
            {
                if (APS168.APS_read_d_output(ModIdx, 0, ref IOStatus) != 0)
                    return;

                //APS168.APS_read_d_output(card, 0, ref IOStatus);
                BitArray myVal = new BitArray(new int[] { (int)IOStatus });
                myVal[port] = sts;
                Byte[] myB = new Byte[4];
                myVal.CopyTo(myB, 0);

                int IOOutStatus = BitConverter.ToInt32(myB, 0);
                //APS168.APS_write_d_output(card, 0, IOOutStatus);
                if (APS168.APS_write_d_output(ModIdx, 0, IOOutStatus) != 0)
                    return;
            }
        }

        public bool ReadIOOut(int ModIdx, int port)
        {
            lock (m_LockObject)
            {
                int IOStatus = 0;
                if (APS168.APS_read_d_output(ModIdx, 0, ref IOStatus) != 0)
                    return false;

                return ((IOStatus & (0x01 << port)) != 0 ? true : false);
            }
        }

        public bool ReadIOIn(int ModIdx, int port)
        {
            int IOStatus = 0;
            try
            {
                lock (m_LockObject)
                {
                    //APS168.APS_read_d_input(card, 0, ref IOStatus);
                    if (APS168.APS_read_d_input(ModIdx, 0, ref IOStatus) != 0)
                        return false;
                    if ((IOStatus & (0x01 << port)) != 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        public static void velocity_move(Int32 Axis_ID)
        {
            Int32 axis_id = Axis_ID;
            Int32 Option = 0;         //Positive direction
            ASYNCALL p = new ASYNCALL();
            double speed_1 = 1000.0;

            APS168.APS_set_axis_param_f(axis_id, (Int32)APS_Define.PRA_STP_DEC, 10000.0);
            APS168.APS_set_axis_param_f(axis_id, (Int32)APS_Define.PRA_CURVE, 0.5); //Set acceleration rate
            APS168.APS_set_axis_param_f(axis_id, (Int32)APS_Define.PRA_ACC, 10000.0); //Set acceleration rate
            APS168.APS_set_axis_param_f(axis_id, (Int32)APS_Define.PRA_DEC, 10000.0); //Set deceleration rate

            //servo on
            APS168.APS_set_servo_on(axis_id, 1);
            Thread.Sleep(500); // Wait stable.

            //go
            APS168.APS_vel(axis_id, Option, speed_1, ref p);
        }

        public static void jog_move_continuous_mode(Int32 Axis_ID)
        {
            APS168.APS_set_axis_param(Axis_ID, (Int32)APS_Define.PRA_JG_MODE, 0);  // Set jog mode
            APS168.APS_set_axis_param(Axis_ID, (Int32)APS_Define.PRA_JG_DIR, 0);  // Set jog direction

            APS168.APS_set_axis_param_f(Axis_ID, (Int32)APS_Define.PRA_JG_CURVE, 0.0);
            APS168.APS_set_axis_param_f(Axis_ID, (Int32)APS_Define.PRA_JG_ACC, 1000.0);
            APS168.APS_set_axis_param_f(Axis_ID, (Int32)APS_Define.PRA_JG_DEC, 1000.0);
            APS168.APS_set_axis_param_f(Axis_ID, (Int32)APS_Define.PRA_JG_VM, 100.0);

            //servo on
            APS168.APS_set_servo_on(Axis_ID, 1);
            Thread.Sleep(500); // Wait stable.

            // Create a rising edge.
            APS168.APS_jog_start(Axis_ID, 0);  //Jog Off
            APS168.APS_jog_start(Axis_ID, 1);  //Jog ON
        }

        public static void p2p_move(Int32 Axis_ID)
        {
            Int32 ret = 0;
            ASYNCALL p = new ASYNCALL();

            // Config speed profile parameters.
            ret = APS168.APS_set_axis_param_f(Axis_ID, (Int32)APS_Define.PRA_SF, 0.5);
            ret = APS168.APS_set_axis_param_f(Axis_ID, (Int32)APS_Define.PRA_ACC, 10000.0);
            ret = APS168.APS_set_axis_param_f(Axis_ID, (Int32)APS_Define.PRA_DEC, 10000.0);
            ret = APS168.APS_set_axis_param_f(Axis_ID, (Int32)APS_Define.PRA_VM, 1000.0);

            //servo on
            APS168.APS_set_servo_on(Axis_ID, 1);
            Thread.Sleep(500); // Wait stable.

            // Start a relative p to p move
            ret = APS168.APS_ptp(Axis_ID, (Int32)APS_Define.OPT_RELATIVE, 1000.0, ref p);
        }

        public static void deceleration_stop(Int32 Axis_ID)
        {
            APS168.APS_set_axis_param_f(Axis_ID, (Int32)APS_Define.PRA_STP_DEC, 10000.0);
            APS168.APS_stop_move(Axis_ID);
        }

        public static void emg_stop(Int32 Axis_ID)
        {
            // Stop immediately
            APS168.APS_emg_stop(Axis_ID);
        }

        public static void interpolation_2D_line(Int32[] Axis_ID_Array)
        {
            double[] PositionArray = new double[] { 1000.0, 2000.0 };
            double TransPara = 0;
            ASYNCALL p = new ASYNCALL();

            // config speed profile
            APS168.APS_set_axis_param_f(Axis_ID_Array[0], (Int32)APS_Define.PRA_SF, 0.5);
            APS168.APS_set_axis_param_f(Axis_ID_Array[0], (Int32)APS_Define.PRA_ACC, 10000.0);
            APS168.APS_set_axis_param_f(Axis_ID_Array[0], (Int32)APS_Define.PRA_DEC, 10000.0);
            APS168.APS_set_axis_param_f(Axis_ID_Array[0], (Int32)APS_Define.PRA_VM, 1000.0);

            //servo on
            APS168.APS_set_servo_on(Axis_ID_Array[0], 1);
            Thread.Sleep(500); // Wait stable.

            //servo on
            APS168.APS_set_servo_on(Axis_ID_Array[1], 1);
            Thread.Sleep(500); // Wait stable.

            // Start a 2 dimension linear interpolation
            APS168.APS_line(
                  2 // I32 Dimension
                , Axis_ID_Array // I32 *Axis_ID_Array
                , (Int32)APS_Define.OPT_RELATIVE  // I32 Option
                , PositionArray // F64 *PositionArray
                , ref TransPara    // F64 *TransPara
                , ref p // ASYNCALL *Wait
            );
        }

        public static void interpolation_2D_arc(Int32[] Axis_ID_Array)
        {
            double[] CenterArray = new double[] { 1000.0, 0.0 };
            double Angle = (90.0 * 3.14159265 / 180.0);
            double TransPara = 0;
            ASYNCALL p = new ASYNCALL();

            // config speed profile
            APS168.APS_set_axis_param_f(Axis_ID_Array[0], (Int32)APS_Define.PRA_SF, 0.5);
            APS168.APS_set_axis_param_f(Axis_ID_Array[0], (Int32)APS_Define.PRA_ACC, 10000.0);
            APS168.APS_set_axis_param_f(Axis_ID_Array[0], (Int32)APS_Define.PRA_DEC, 10000.0);
            APS168.APS_set_axis_param_f(Axis_ID_Array[0], (Int32)APS_Define.PRA_VM, 1000.0);

            //servo on
            APS168.APS_set_servo_on(Axis_ID_Array[0], 1);
            Thread.Sleep(500); // Wait stable.

            //servo on
            APS168.APS_set_servo_on(Axis_ID_Array[1], 1);
            Thread.Sleep(500); // Wait stable.

            // Start a 2 dimension linear interpolation
            APS168.APS_arc2_ca(
                  Axis_ID_Array // I32 *Axis_ID_Array
                , (Int32)APS_Define.OPT_RELATIVE  // I32 Option
                , CenterArray   // F64 *CenterArray
                , Angle         // F64 Angle
                , ref TransPara    // F64 *TransPara
                , ref p // ASYNCALL *Wait 
            );
        }

        public static void interpolation_3D_arc(Int32[] Axis_ID_Array)
        {
            double[] CenterArray = new double[] { 1000.0, 1000.0, 0 };
            double[] EndArray = new double[] { 1000.0, 1000.0, 1000.0 * Math.Sqrt(2) };

            Int16 Dir = 0;
            double TransPara = 0;
            Int32 i;
            ASYNCALL p = new ASYNCALL();

            // config speed profile
            APS168.APS_set_axis_param_f(Axis_ID_Array[0], (Int32)APS_Define.PRA_SF, 0.5);
            APS168.APS_set_axis_param_f(Axis_ID_Array[0], (Int32)APS_Define.PRA_ACC, 10000.0);
            APS168.APS_set_axis_param_f(Axis_ID_Array[0], (Int32)APS_Define.PRA_DEC, 10000.0);
            APS168.APS_set_axis_param_f(Axis_ID_Array[0], (Int32)APS_Define.PRA_VM, 2000.0);

            //Check servo on or not
            for (i = 0; i < 3; i++)
            {
                APS168.APS_set_servo_on(Axis_ID_Array[i], 1);
            }

            Thread.Sleep(500); // Wait stable.

            APS168.APS_arc3_ce(
                  Axis_ID_Array // I32 *Axis_ID_Array
                , (Int32)APS_Define.OPT_RELATIVE  // I32 Option
                , CenterArray   // F64 *CenterArray
                , EndArray      // F64 *EndArray
                , Dir           // I16 Dir
                , ref TransPara    //F64 *TransPara
                , ref p // ASYNCALL *Wait 
            );
        }

        public static void interpolation_3D_helical(Int32[] Axis_ID_Array)
        {
            double[] CenterArray = new double[] { 1000.0, 1000.0, 0 };
            double[] NormalArray = new double[] { 0, 0, 1 };
            double Angle = (720.0 * 3.14159265 / 180.0);
            double DeltaH = 500.0;
            double FinalR = 200.0;
            double TransPara = 0;
            Int16 i;
            ASYNCALL p = new ASYNCALL();

            // config speed profile
            APS168.APS_set_axis_param_f(Axis_ID_Array[0], (Int32)APS_Define.PRA_SF, 0.5);
            APS168.APS_set_axis_param_f(Axis_ID_Array[0], (Int32)APS_Define.PRA_ACC, 10000.0);
            APS168.APS_set_axis_param_f(Axis_ID_Array[0], (Int32)APS_Define.PRA_DEC, 10000.0);
            APS168.APS_set_axis_param_f(Axis_ID_Array[0], (Int32)APS_Define.PRA_VM, 2000.0);

            //Check servo on or not
            for (i = 0; i < 3; i++)
            {
                APS168.APS_set_servo_on(Axis_ID_Array[i], 1);
            }

            Thread.Sleep(500); // Wait stable.

            APS168.APS_spiral_ca(
                  Axis_ID_Array // I32 *Axis_ID_Array
                , (Int32)APS_Define.OPT_RELATIVE  // I32 Option
                , CenterArray   // F64 *CenterArray
                , NormalArray   // F64 *NormalArray
                , Angle         // F64 Angle
                , DeltaH        // F64 DeltaH
                , FinalR        // F64 FinalR
                , ref TransPara    // F64 *TransPara
                , ref p  // ASYNCALL *Wait 
            );
        }

        public static void contour_2D(Int32[] Axis_ID_Array)
        {
            ASYNCALL p = new ASYNCALL();
            Int16 i;
            double TransPara = 0;
            double[] PositionArray = new double[] { 1000,    0,    // segment 1
		                                               0, 1000 };  // segment 3
            double[] CenterArray = new double[] { 1000,  500,     // segment 2 (center)
	                                                  0,  500 };  // segment 4 (center)
            double[] EndArray = new double[] { 1000, 1000,        // segment 2 (end)
	                                              0,    0 };      // segment 4 (end)
            double[] m_PositionArray = new double[] { 0, 1000 };
            double[] m_CenterArray = new double[] { 0, 500 };
            double[] m_EndArray = new double[] { 0, 0 };

            //Check servo on or not
            for (i = 0; i < 2; i++)
            {
                APS168.APS_set_servo_on(Axis_ID_Array[i], 1);
            }
            Thread.Sleep(500); // Wait stable.

            // Execute 4 interpolation move useing bufferd mode. Note option using "ITP_OPT_BUFFERED"

            APS168.APS_line_all(2, Axis_ID_Array, (Int32)APS_Define.OPT_ABSOLUTE | (Int32)APS_Define.ITP_OPT_BUFFERED, PositionArray, ref TransPara
                , 0.0 // F64 Vs
                , 1000.0 // F64 Vm
                , 1000.0 // F64 Ve
                , 10000.0 // F64 Acc
                , 10000.0 // F64 Dec
                , 0.0 // F64 SFac
                , ref p // ASYNCALL *Wait 
            );

            APS168.APS_arc2_ce_all(Axis_ID_Array, (Int32)APS_Define.OPT_ABSOLUTE | (Int32)APS_Define.ITP_OPT_BUFFERED, CenterArray, EndArray, 0, ref TransPara
                , 1000.0 // F64 Vs
                , 1000.0 // F64 Vm
                , 1000.0 // F64 Ve
                , 10000.0 // F64 Acc
                , 10000.0 // F64 Dec
                , 0.0 // F64 SFac
                , ref p // ASYNCALL *Wait 
            );

            APS168.APS_line_all(2, Axis_ID_Array, (Int32)APS_Define.OPT_ABSOLUTE | (Int32)APS_Define.ITP_OPT_BUFFERED, m_PositionArray, ref TransPara
                , 1000.0 // F64 Vs
                , 1000.0 // F64 Vm
                , 1000.0 // F64 Ve
                , 10000.0 // F64 Acc
                , 10000.0 // F64 Dec
                , 0.0 // F64 SFac
                , ref p // ASYNCALL *Wait 
            );

            APS168.APS_arc2_ce_all(Axis_ID_Array, (Int32)APS_Define.OPT_ABSOLUTE | (Int32)APS_Define.ITP_OPT_BUFFERED, m_CenterArray, m_EndArray, 0, ref TransPara
                , 1000.0 // F64 Vs
                , 1000.0 // F64 Vm
                , 0.0 // F64 Ve
                , 10000.0 // F64 Acc
                , 10000.0 // F64 Dec
                , 0.0 // F64 SFac
                , ref p // ASYNCALL *Wait 
            );
        }

        public static void point_table_2D(Int32 Board_ID, Int32[] Axis_ID_Array)
        {
            Int32 boardId = Board_ID;
            Int32 ptbId = 0;          //Point table id 0
            Int32 dimension = 2;      //2D point table

            PTSTS Status = new PTSTS();
            PTLINE Line = new PTLINE();
            PTA2CA Arc2 = new PTA2CA();

            Int32 doChannel = 0; //Do channel 0
            Int32 doOn = 0;
            Int32 doOff = 1;
            Int32 i = 0;
            Int32 ret = 0;


            //Check servo on or not
            for (i = 0; i < dimension; i++)
            {
                ret = APS168.APS_set_servo_on(Axis_ID_Array[i], 1);
            }

            Thread.Sleep(500); // Wait stable.

            //Enable point table
            ret = APS168.APS_pt_disable(boardId, ptbId);
            ret = APS168.APS_pt_enable(boardId, ptbId, dimension, Axis_ID_Array);

            //Configuration
            ret = APS168.APS_pt_set_absolute(boardId, ptbId); //Set to absolute mode
            ret = APS168.APS_pt_set_trans_buffered(boardId, ptbId); //Set to buffer mode
            ret = APS168.APS_pt_set_acc(boardId, ptbId, 10000); //Set acc
            ret = APS168.APS_pt_set_dec(boardId, ptbId, 10000); //Set dec

            //Get status
            //BitSts;	//b0: Is PTB work? [1:working, 0:Stopped]
            //b1: Is point buffer full? [1:full, 0:not full]
            //b2: Is point buffer empty? [1:empty, 0:not empty]
            //b3~b5: reserved

            ret = APS168.APS_get_pt_status(boardId, ptbId, ref Status);
            if ((Status.BitSts & 0x02) == 0) //Buffer is not Full
            {
                //Set 1st move profile
                ret = APS168.APS_pt_set_vm(boardId, ptbId, 10000); //Set vm to 10000
                ret = APS168.APS_pt_set_ve(boardId, ptbId, 5000); //Set ve to 5000

                //Set do command to sync with 1st point
                ret = APS168.APS_pt_ext_set_do_ch(boardId, ptbId, doChannel, doOn); //Set Do channel 0 to on

                //Set pt arc data
                Arc2.Center = new double[] { 1000, 1000 };
                Arc2.Angle = (180) * 3.14159265 / 180.0;   //180 degree
                Arc2.Index = new Byte[] { 0, 1 };

                //Push 1st point to buffer
                ret = APS168.APS_pt_arc2_ca(boardId, ptbId, ref Arc2, ref Status);
            }

            //Get status
            ret = APS168.APS_get_pt_status(boardId, ptbId, ref Status);
            if ((Status.BitSts & 0x02) == 0) //Buffer is not Full
            {
                //Set 2nd move profile
                ret = APS168.APS_pt_set_vm(boardId, ptbId, 12000); //Set vm to 12000
                ret = APS168.APS_pt_set_ve(boardId, ptbId, 6000); //Set ve to 6000

                //Set do command to sync with 2nd point
                ret = APS168.APS_pt_ext_set_do_ch(boardId, ptbId, doChannel, doOff); //Set Do channel 0 to on

                //Set pt line data
                Line.Dim = 2;
                Line.Pos = new Double[] { 0, 0, 0, 0, 0, 0 };

                //Push 2nd point to buffer
                ret = APS168.APS_pt_line(boardId, ptbId, ref Line, ref Status);
            }

            //Get status
            ret = APS168.APS_get_pt_status(boardId, ptbId, ref Status);
            if ((Status.BitSts & 0x02) == 0) //Buffer is not Full
            {
                //Set 3rd move profile
                ret = APS168.APS_pt_set_vm(boardId, ptbId, 10000); //Set vm to 10000
                ret = APS168.APS_pt_set_ve(boardId, ptbId, 5000); //Set ve to 5000

                //Set do command to sync with 3rd point
                ret = APS168.APS_pt_ext_set_do_ch(boardId, ptbId, doChannel, doOn); //Set Do channel 0 to off

                //Set pt arc data
                Arc2.Center = new Double[] { 1000, 1000 };
                Arc2.Angle = (180) * 3.14159265 / 180.0; //180 degree
                Arc2.Index = new Byte[] { 0, 1 };

                //Push 3rd point to buffer
                ret = APS168.APS_pt_arc2_ca(boardId, ptbId, ref Arc2, ref Status);
            }

            //Get status
            ret = APS168.APS_get_pt_status(boardId, ptbId, ref Status);
            if ((Status.BitSts & 0x02) == 0) //Buffer is not Full
            {
                //Set 4th move profile
                ret = APS168.APS_pt_set_vm(boardId, ptbId, 12000); //Set vm to 12000
                ret = APS168.APS_pt_set_ve(boardId, ptbId, 500); //Set ve to 500

                //Set do command to sync with 4th point
                ret = APS168.APS_pt_ext_set_do_ch(boardId, ptbId, doChannel, doOff); //Set Do channel 0 to on

                //Set pt line data
                Line.Dim = 2;
                Line.Pos = new Double[] { 0, 0, 0, 0, 0, 0 };

                //Push 4th point to buffer
                ret = APS168.APS_pt_line(boardId, ptbId, ref Line, ref Status);
            }

            ret = APS168.APS_pt_start(boardId, ptbId);
        }

        public static void gear_move(Int32[] Axis_ID_Array)
        {
            Int32 masterAxis = Axis_ID_Array[0];
            Int32 slaveAxis = Axis_ID_Array[1];
            Int32 i = 0;
            Int32 status = 0;
            ASYNCALL p = new ASYNCALL();
            Int32 ret = 0;

            //Check servo on or not
            for (i = 0; i < 2; i++)
            {
                APS168.APS_set_servo_on(Axis_ID_Array[i], 1);
            }
            Thread.Sleep(500); // Wait stable.

            APS168.APS_get_gear_status(slaveAxis, ref status);
            //status == 0, Disable mode
            //status == 1, Gear mode
            //status == 2. Gantry mode

            if (status == 0)
            {
                //Select master Axis command to be slaveAxis's master
                ret = APS168.APS_set_axis_param(slaveAxis, (Int32)APS_Define.PRA_GEAR_MASTER, masterAxis);

                //Set Gear engage rate
                ret = APS168.APS_set_axis_param_f(slaveAxis, (Int32)APS_Define.PRA_GEAR_ENGAGE_RATE, 10000.0);

                //set Gear ratio
                ret = APS168.APS_set_axis_param_f(slaveAxis, (Int32)APS_Define.PRA_GEAR_RATIO, 2.0);

                //Set E-gear gantry mode protection level 1
                ret = APS168.APS_set_axis_param_f(slaveAxis, (Int32)APS_Define.PRA_GANTRY_PROTECT_1, 10000.0);

                //Set E-gear gantry mode protection level 2
                ret = APS168.APS_set_axis_param_f(slaveAxis, (Int32)APS_Define.PRA_GANTRY_PROTECT_2, 20000.0);

                //Set to Standard mode (0: Disable, 1: Standard mode, 2: Gantry mode.)
                ret = APS168.APS_start_gear(slaveAxis, 1);
            }

            //Start master axis relative p to p move
            ret = APS168.APS_ptp(masterAxis, (Int32)APS_Define.OPT_RELATIVE, 1000.0, ref p);

            //Wait for motion done
            Thread.Sleep(1200);

            //Disable Standard mode (0: Disable, 1: Standard mode, 2: Gantry mode.)
            ret = APS168.APS_start_gear(slaveAxis, 0);
        }

        public static Int32 interrupt_move(Int32 Board_ID, Int32 Axis_num)
        {
            // This example shows how interrupt functions work.
            Int32 board_id = Board_ID;
            Int32 int_no;      // Interrupt number
            Int32 return_code; // function return code
            Int32 item = Axis_num;    // Axis #? interrupt
            Int32 factor = 12; // factor number = 12 IMDN interrupt

            // Step 1: set interrupt factor ( factor = IMDN )
            // ³]©w­nµ¥«Ýªº¤¤Â_¨Æ¥ó
            int_no = APS168.APS_set_int_factor(board_id, item, factor, 1);

            // Step 2: set interrupt main switch 
            // ³]©w¤¤Â_Á`¶}Ãö
            APS168.APS_int_enable(board_id, 1); // Enable the interrupt main switch

            // Step 3: wait interrupt.
            // µ¥«Ý¤¤Â_Ä²µo
            return_code = APS168.APS_wait_single_int(int_no, -1); //Wait interrupt

            if (return_code == (Int32)APS_Define.ERR_NoError)
            { //Interrupt occurred	
                //Step 4: ­«¸m¤¤Â_¬°¬°Ä²µoª¬ºA
                APS168.APS_reset_int(int_no);
            }

            // Step 5: Disable interrupt at the end of program.
            // Ãö³¬¤¤Â_¨Æ¥ó©M¤¤Â_Á`¶}Ãö
            APS168.APS_set_int_factor(board_id, item, factor, 0);
            APS168.APS_int_enable(board_id, 0);

            return return_code;
        }

        public static void DIO_set(Int32 Board_ID)
        {
            Int32 __MAX_DO_CH = (24);
            Int32 __MAX_DI_CH = (24);

            Int32 digital_output_value = 0;
            Int32 digital_input_value = 0;

            Int32[] do_ch = new Int32[__MAX_DO_CH];
            Int32[] di_ch = new Int32[__MAX_DI_CH];
            Int16 i;

            //****** Read digital output channels *****************************
            APS168.APS_read_d_output(Board_ID
                , 0                     // I32 DO_Group
                , ref digital_output_value // I32 *DO_Data
            );

            for (i = 0; i < __MAX_DO_CH; i++)
                do_ch[i] = ((digital_output_value >> i) & 1);

            //************ Write digital output channels examples *************
            do_ch[0] = 1;  // set 0 or 1
            do_ch[2] = 1;  // set 0 or 1
            do_ch[4] = 1;  // set 0 or 1

            digital_output_value = 0;
            for (i = 0; i < __MAX_DO_CH; i++)
                digital_output_value |= (do_ch[i] << i);


            APS168.APS_write_d_output(Board_ID
                , 0                     // I32 DO_Group
                , digital_output_value  // I32 DO_Data
            );
            //*******************************************************************

            //**** Read digital input channels **********************************
            APS168.APS_read_d_input(Board_ID
                , 0                    // I32 DI_Group
                , ref digital_input_value // I32 *DI_Data
            );

            for (i = 0; i < __MAX_DI_CH; i++)
                di_ch[i] = ((digital_input_value >> i) & 1);
            //********************************************************************
        }

        public static void AIO_set(Int32 Board_ID)
        {
            Int32 Channel_No = 0;
            double analog_input_volt = 0.0;
            double analog_output_volt = 0.0;

            APS168.APS_read_a_input_value(Board_ID, Channel_No, ref analog_input_volt);

            analog_output_volt = 1.5; // 1.5 volt
            APS168.APS_write_a_output_value(Board_ID, Channel_No, analog_output_volt);

            APS168.APS_read_a_output_value(Board_ID, Channel_No, ref analog_output_volt);
        }
    }
}
