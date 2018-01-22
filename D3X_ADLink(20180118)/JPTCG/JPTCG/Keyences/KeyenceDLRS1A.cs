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

namespace Keyences
{
    public class KeyenceDLRS1A
    {

        public String RecivedMsg = "";
        private SerialPort port = null;
        public string MyPortName = "COM1";
        private string MyName = "DLRS1A";
        public double OriginValue = 0;

        public KeyenceDLRS1A(string myName)
        {
            MyName = myName;
            port = new SerialPort();
            port.DataReceived += port_DataReceived;
        }
        ~KeyenceDLRS1A()
        {
            Disconnect();
        }
        void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                String str0 = port.ReadExisting();
               // String str = port.ReadLine();                
                //RecivedMsg = str;
                RecivedMsg = str0;
                //port.
            }
            catch (Exception)
            {

            }
        }

        public bool Connect(string PortName)
        {
            try
            {
                MyPortName = PortName;
                //port = new SerialPort();
                if (port.IsOpen)
                    port.Close();
                port.PortName = PortName;//"COM" + PortNum.ToString();
                port.BaudRate = 9600;
                port.StopBits = System.IO.Ports.StopBits.One;
                port.DataBits = 8;
                port.Parity = System.IO.Ports.Parity.None;
                port.Handshake = Handshake.None;
                //port.DataReceived += port_DataReceived;               
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
                    //port = null;
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

        public double Read()
        {
            if (!port.IsOpen)
                return 0;
            //Read All data
            //string data = "4D 30 0D 0A";//20171220
            string data = "02 4D 45 41 53 55 52 45 03";           
            byte[] hex = GetByteArray(data);            
            //port.DiscardInBuffer();
            //port.DiscardOutBuffer();
            
            double tempDou = 0;
            for (int i =0 ;i <3;i++)
            {
                port.Write(hex, 0, hex.Length);
                Thread.Sleep(5*i);
                //String str0 = port.ReadExisting();
                //Console.Write(str0);
                //String str = port.ReadLine();

                string tempStr = GetReply(1);
                string tempStr1 = "";               
                try
                {
                    //tempStr1 = tempStr.Substring(3);
                    tempStr1 = tempStr.Substring(1,tempStr.Length-2);
                    tempDou = double.Parse(tempStr1);
                }
                catch (Exception e)
                {
                    tempDou = 0;
                }

                if (tempDou != 0)
                    break;               
            }

            return tempDou;
        }

        public void SaveSettings(string fileName)
        {
            string Header = "";

            Header = "DLRS_" + MyName;
            FileOperation.SaveData(fileName, Header, "ComPort", MyPortName);
           
            FileOperation.SaveData(fileName, Header, "OriginValue", OriginValue.ToString());           
        }
        public void LoadSettings(string fileName)
        {
            string Header = "";
            string strread = "";

            Header = "DLRS_" + MyName;

            FileOperation.ReadData(fileName, Header, "ComPort", ref strread);
            if (strread != "0")
                MyPortName = strread;           

            if (Para.MachineOnline)
            {
                if (!Connect(MyPortName))
                {
                    //MessageBox.Show("DLRS " + MyName + " can't be connected.");//20170222
                }
            }

            FileOperation.ReadData(fileName, Header, "OriginValue", ref strread);

            OriginValue = double.Parse(strread);
        }
    }
}
