using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using Common;
using JPTCG.Common;

namespace JPTCG.HeightSensor
{
    public class OptexCD33
    {
        public String RecivedMsg = "";
        private SerialPort port = null;
        public string MyPortName = "COM3";
        private string MyName = "OptexCD33";
        public double OriginValue = 0;

        private string name = "";
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public OptexCD33(string myName)
        {
            //MyName = myName;
            Name = myName;
            port = new SerialPort();//20171230
            port.DataReceived += port_DataReceived;
        }

        ~OptexCD33()
        {
            Disconnect();
        }

        List<byte>[] buf = new List<byte>[4096];
        void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                String str0 = port.ReadExisting();
                //String str = port.ReadLine();                
                //RecivedMsg = str;
                int len = str0.Length;
                RecivedMsg = str0;
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
                if (port.IsOpen)
                    port.Close();
                port.PortName = PortName;
                port.BaudRate = 9600;
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

        public void Disconnect()
        {
            try
            {
                if (port != null && port.IsOpen)               
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
            //string data = "4D 30 0D 0A";//keyence
            string data = "02 4D 45 41 53 55 52 45 03";
            byte[] hex = GetByteArray(data); 
            double tempDou = 0;
            for (int i = 0; i < 3; i++)
            {
                port.Write(hex, 0, hex.Length);
                Thread.Sleep(5 * i);               
                string tempStr = GetReply(1);
                string tempStr1 = "";
                try
                {
                    tempStr1 = tempStr.Substring(1, tempStr.Length - 2);
                    tempDou = double.Parse(tempStr1);
                    break;
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

        public int SetZero()
        {
            if (!port.IsOpen)
                return -1;
            string externCmd = "02 4D 46 20 54 45 41 43 48 03";
            string zeroCmd = "02 4F 4E 20 37 30 30 03";
            byte[] hex1 = GetByteArray(externCmd);
            byte[] hex2 = GetByteArray(zeroCmd);

            for (int i = 0; i < 2; i++)
            {
                port.Write(hex1, 0, hex1.Length);
                Thread.Sleep(50);
                port.Write(hex2, 0, hex2.Length);
            }

            return 0;
        }

        public void SaveSettings(string fileName)
        {
            string Header = "";

            Header = "CD33_" + Name;
            FileOperation.SaveData(fileName, Header, "ComPort", MyPortName);

            FileOperation.SaveData(fileName, Header, "OriginValue", OriginValue.ToString());
        }

        public void LoadSettings(string fileName)
        {
            string Header = "";
            string strread = "";

            //Header = "DLRS_" + MyName;
            Header = "CD33_" + Name;

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
