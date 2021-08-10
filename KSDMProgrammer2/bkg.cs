using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace KSDMProgrammer2
{
    public class Bkg : EventArgs
    {
        public string potential;
        public bool found;
        public string typeFound = "";
        public string[] nameArray;
        public delegate void Function();

        public void Scan()
        {
            TaskScan();
        }

        public void TaskMethod(Function call)
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
