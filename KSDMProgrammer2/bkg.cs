using System;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

namespace KSDMProgrammer2
{

    
    public class Bkg : EventArgs
    {
        private static readonly string pth = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ksdm-temp\\");
        public string potential;
        public bool found;
        public string typeFound = "";
        public string[] nameArray;
        public static SerialPort p;
        public delegate void Function();

        public void Scan()
        {
            TaskScan();
        }

        public static void TaskMethod(Function call)
        {
            _ = Task.Run(() =>
            {
                call();
            });
        }

        private void TaskScan()
        {
            _ = Task.Run(() =>
            {
                OnBegin(new EventArgs());
                Debug.WriteLine("Begin Scan");

                nameArray = System.IO.Ports.SerialPort.GetPortNames();      // get a list of available ports
                found = false;

                if (nameArray.Length > 1)
                {
                    nameArray = Helper.Swap(nameArray, 0, nameArray.Length - 1);
                }
                foreach (string b in nameArray)
                {
                    string temp = Exe.SerialPoke(b);
                    if (temp.Contains("ksdm3"))
                    {
                        found = true;
                        if (temp.Contains("avr"))
                        {
                            if (temp.Contains("sportplus"))
                                KSDM3.submodel = "sp";
                            else
                                KSDM3.submodel = "3";

                            KSDM3.cpu = "avr";
                            potential = b;
                            break;
                        }
                        else if (temp.Contains("rp2040"))
                        {
                            if (temp.Contains("sportplus"))
                                KSDM3.submodel = "sp";
                            else
                                KSDM3.submodel = "3";

                            KSDM3.cpu = "rp2040";
                            potential = b;
                            break;
                        }
                    }
                    continue;
                }
                KSDM3.port = potential;

                OnComplete(new EventArgs());
            });
        }

        public static void getDebugLog(string port)
        {
            if (KSDM3.cpu == "rp2040") { 
                p = new SerialPort();
                p.PortName = port;
                p.BaudRate = 115200;
                p.Parity = Parity.None;
                p.StopBits = StopBits.One;
                p.Handshake = Handshake.XOnXOff;
                p.RtsEnable = true;
                p.DtrEnable = true;
                p.ReadTimeout = 5000;
                TaskMethod(SerialDoTask);
            }
        }

        public static void SerialDoTask()
        {
            string response;
            p.Open();
            while (true)
            {
                if (p.IsOpen)
                {
                    p.WriteLine("dl");
                    break;
                }
            }
            while (true)
            {
                Thread.Sleep(150);
                if (p.IsOpen)
                {
                    response = "";
                    if (p.BytesToRead > 0)
                        response = p.ReadExisting();

                    if (response.Contains("#eof"))
                    {
                        p.Close();
                        break;
                    }
                }
            }
            File.WriteAllText(pth + "KSDMLogFile.log", response);
        }

        protected virtual void OnBegin(EventArgs e)
        {
            MainWindow.ScanBegin?.Invoke(this, e);
        }
        protected virtual void OnComplete(EventArgs e)
        {
            MainWindow.ScanComplete?.Invoke(this, e);
        }
    }
    
}
