using Common;
using JPTCG.Common;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JPTCG.LightTiming
{
    public class LampLDLS
    {
        public String RecivedMsg = "";       
        private SerialPort port = null;
        public string MyPortName = "COM1";
        private string MyName = "LDLS";
        public double OriginValue = 0;
        public int RunDaysValue = 0;
        public int RunHoursValue = 0;
        public int RunMinsValue = 0;
        public int RunSecondsValue = 0;

        public LampLDLS(string myName)
        {
            MyName = myName;
            port = new SerialPort();
            port.DataReceived += comPort_DataReceived;
        }
        ~LampLDLS()
        {
            Disconnect();
        }

        //List<byte>[] buf = new List<byte>[4096];
        //void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        //{
        //    try
        //    {
        //        String str0 = port.ReadExisting();               
        //        //String str = port.ReadLine();                
        //        //RecivedMsg = str;
        //        int len = str0.Length;
        //        RecivedMsg = str0; 
        //    }
        //    catch (Exception)
        //    {

        //    }
        //}

        private List<byte> buffer = new List<byte>(4096);
        private byte[] oneData = new byte[9];
        public void comPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {              
                Byte[] ReceivedData = new Byte[port.BytesToRead];
                port.Read(ReceivedData, 0, ReceivedData.Length);
                buffer.AddRange(ReceivedData);

                if(buffer.Count > 8 && buffer[1] ==0x03)
                {
                    int len = buffer[2];
                    byte[] checkResult = new byte[2];
                    buffer.CopyTo(0, oneData, 0, len + 5);
                    byte[] dataToCheck = new byte[oneData.Length - 2];
                    System.Array.Copy(oneData, dataToCheck, dataToCheck.Length);
                    checkResult = CRC16(dataToCheck);

                    if (checkResult[0] != oneData[8] || checkResult[1] != oneData[7])
                    {
                        buffer.RemoveRange(0, len + 5);
                    }
                    else
                    {
                        RecivedMsg = Convert.ToString(oneData[5], 16) + Convert.ToString(oneData[6], 16) + Convert.ToString(oneData[3], 16) + Convert.ToString(oneData[4], 16);
                        buffer.RemoveRange(0, len + 5);
                    }
                    //RecivedMsg = Convert.ToString(oneData[5], 16) + Convert.ToString(oneData[6], 16) + Convert.ToString(oneData[3], 16) + Convert.ToString(oneData[4], 16);
                    //buffer.RemoveRange(0, len + 5);
                }
                else if (buffer.Count > 7 && buffer[0] == 0x01 && buffer[1] == 0x10)
                {
                    //int len = buffer[2];
                    buffer.RemoveRange(0, 16);
                    buffer.CopyTo(0, oneData, 0, 9);
                    RecivedMsg = Convert.ToString(oneData[5], 16) + Convert.ToString(oneData[6], 16) + Convert.ToString(oneData[3], 16) + Convert.ToString(oneData[4], 16);
                    buffer.RemoveRange(0, 9);
                }
                
            }
            catch (Exception ex)
            {
                
            }       
        }
     
        public bool Connect(string PortName)
        {
            try
            {
                MyPortName = PortName;              
                if (port.IsOpen)
                    port.Close();             
                port.PortName = PortName;
                port.BaudRate = 115200;
                port.StopBits = System.IO.Ports.StopBits.One;
                port.DataBits = 8;
                port.Parity = System.IO.Ports.Parity.None;
                port.Handshake = Handshake.None;
                       
                port.Open();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void  Disconnect()
        {
            try
            {
                if (port != null &&
                    port.IsOpen)
                {
                    port.Close();                  
                }
            }
            catch (Exception ex)
            {
                return;
            }           
        }

        private string GetReply(int timeout_seconds)
        {
            RecivedMsg = "";

            bool bTimeout = false;
            DateTime st = DateTime.Now;
            TimeSpan ts = DateTime.Now - st;
            while (!bTimeout)
            {
                if (RecivedMsg != "")
                    break;
                ts = DateTime.Now - st;
                bTimeout = ts.Seconds >= timeout_seconds;
                Thread.Sleep(5);
            }
            return RecivedMsg;
        }

        private byte[] GetByteArray(string hex)
        {
            string[] ssArray = hex.Split(' ');
            List<byte> bytList = new List<byte>();
            foreach (var s in ssArray)
            {
                //将十六进制的字符串转换成数值
                bytList.Add(Convert.ToByte(s, 16));
            }
            //返回字节数组
            return bytList.ToArray();
        }

        public bool IsConnected()
        {
            if (port == null)
                return false;

            return port.IsOpen;
        }
       
        public int ReadLight()
        { 
            if (!port.IsOpen)
                return 0;
            string data = "01 03 80 00 00 02 ED CB";           
            byte[] hex = GetByteArray(data);            
   
            int LightCounts = 0;

            for (int i =0 ;i <3;i++)
            {
                port.Write(hex, 0, hex.Length);
                Thread.Sleep(100);
                string tempStr = RecivedMsg;
                //RecivedMsg = "";               
                try
                {                                    
                    LightCounts = Convert.ToInt32(tempStr, 16);
                    RecivedMsg = "";
                }
                catch (Exception e)
                {
                    LightCounts = 0;
                }

                if (LightCounts != 0)
                    break;
            }

            return LightCounts;
        }

        public int WriteCommand(Int32 val)
        {
            if (!port.IsOpen)
                return 0;
            string strCmd = "";
            string head = "01 10 80 00 00 02 04";
            string data = val.ToString("X8");          
            string checkData = "";            
            checkData = head + " " + data.Substring(data.Length - 4, 2) + " " + data.Substring(data.Length - 2, 2) + " " + data.Substring(data.Length - 8, 2) + " " + data.Substring(data.Length - 6, 2);
            byte[] hex = GetByteArray(checkData);
            strCmd = checkData + " " + CRC16(hex)[1].ToString("X2") + " " + CRC16(hex)[0].ToString("X2");
            byte[] hexCmd = GetByteArray(strCmd);
            for (int i = 0; i < 2; i++)
            {
                port.Write(hexCmd, 0, hexCmd.Length);
            }
            return 0;
        }

        public byte[] CRC16(byte[] cmd)
        {
            string crc16 = "";
            byte CRC16Lo;
            byte CRC16Hi;                                       //CRC寄存器 
            byte CL; byte CH;                                   //多项式码&HA001 
            byte SaveHi; byte SaveLo;
            int Flag;
            CRC16Lo = 0xFF;
            CRC16Hi = 0xFF;
            CL = 0x01;
            CH = 0xA0;
            for (int i = 0; i < cmd.Length; i++)
            {
                //CRC16Lo = (byte)(CRC16Lo ^ testCmd[i]); //每一个数据与CRC寄存器进行异或 
                CRC16Lo = (byte)(CRC16Lo ^ cmd[i]);
                for (Flag = 0; Flag <= 7; Flag++)
                {
                    SaveHi = CRC16Hi;
                    SaveLo = CRC16Lo;
                    CRC16Hi = (byte)(CRC16Hi >> 1);
                    CRC16Lo = (byte)(CRC16Lo >> 1);
                    if ((SaveHi & 0x01) == 0x01)            //如果高位字节最后一位为1 
                    {
                        CRC16Lo = (byte)(CRC16Lo | 0x80);   //则低位字节右移后前面补1 
                    }                                       //否则自动补0 
                    if ((SaveLo & 0x01) == 0x01)            //如果LSB为1，则与多项式码进行异或 
                    {
                        CRC16Hi = (byte)(CRC16Hi ^ CH);
                        CRC16Lo = (byte)(CRC16Lo ^ CL);
                    }
                }
            }
            byte[] CRCResult = new byte[2];
            CRCResult[0] = CRC16Hi;                        //CRC高位 
            CRCResult[1] = CRC16Lo;                        //CRC低位 
            crc16 = CRCResult[1].ToString("X2") + " " + CRCResult[0].ToString("X2");
            //return crc16;
            return CRCResult;
        }

        public void SaveSettings(string fileName)
        {
            string Header = "";
            Header = "LDLS_" + MyName;
            //int timeRocoder = System.Environment.TickCount;
            //TimeSpan tp = DateTime.Now - Para.SystemRunTime;
            FileOperation.SaveData(fileName, Header, "ComPort", MyPortName);
            FileOperation.SaveData(fileName, Header, "RunDaysValue", Para.rtDays.ToString());
            FileOperation.SaveData(fileName, Header, "RunHoursValue", Para.rtHours.ToString());
            FileOperation.SaveData(fileName, Header, "RunMinsValue", Para.rtMins.ToString());
            //tp = DateTime.Now - Para.GetLightSourceTime;//20180110
            //if (tp.TotalMinutes > 10)
            //{
            Para.rtSeconds = Para.myMain.LDLS1.ReadLight();                
            FileOperation.SaveData(fileName, Header, "RunSecondsValue", Para.rtSeconds.ToString());
            //}
        }
            

        public void LoadSettings(string fileName)
        {
            string Header = "";
            string strread = "";

            Header = "LDLS_" + MyName;

            FileOperation.ReadData(fileName, Header, "ComPort", ref strread);
            if (strread != "0")
                MyPortName = strread;
            FileOperation.ReadData(fileName, Header, "RunDaysValue", ref strread);
            Para.rtDays = int.Parse(strread);
            FileOperation.ReadData(fileName, Header, "RunHoursValue", ref strread);
            Para.rtHours = double.Parse(strread);
            FileOperation.ReadData(fileName, Header, "RunMinsValue", ref strread);
            Para.rtMins = double.Parse(strread);
            FileOperation.ReadData(fileName, Header, "RunSecondsValue", ref strread);
            Para.rtSeconds = double.Parse(strread);

            if (Para.MachineOnline)
            {
                if (!Connect(MyPortName))
                {
                    //MessageBox.Show("DLRS " + MyName + " can't be connected.");//20170222
                }
            }

            //FileOperation.ReadData(fileName, Header, "OriginValue", ref strread);
            //OriginValue = double.Parse(strread);
        }
    }
}
