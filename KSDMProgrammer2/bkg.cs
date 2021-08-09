using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace KSDMProgrammer2
{
    public class bkg : EventArgs
    {
        public string potential;
        public bool found;
        public string typeFound = "";
        public string[] nameArray;
        public delegate void Function();

        public void Scan()
        {
            t_scan();
        }
        
        public void t_method(Function call)
        {
            Task.Run(()=> {
                call();
            });
        }

        private void t_scan()
        {
            Task.Run(() =>
            {
                onBegin(new EventArgs());
                Debug.WriteLine("Begin Scan");

                nameArray = System.IO.Ports.SerialPort.GetPortNames();      // get a list of available ports
                found = false;
                string tr;

                // convoluted way to move COM1 to end of list, usually it's not what we're looking for but sometimes it can be.
                if (nameArray.Length > 1)
                {
                    tr = nameArray[0];
                    nameArray[0] = nameArray[nameArray.Length - 1];
                    nameArray[nameArray.Length - 1] = tr;
                }
                foreach (string b in nameArray)
                {
                    string temp = exe.serialPoke(b);
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

                onComplete(new EventArgs());
            });
        }

        protected virtual void onBegin(EventArgs e)
        {
            MainWindow.ScanBegin?.Invoke(this, e);
        }
        protected virtual void onComplete(EventArgs e)
        {
            MainWindow.ScanComplete?.Invoke(this, e);
        }
        
    }

    public static class KSDM3
    {
        public static string cpu;
        public static string submodel;
        public static string port;
    }
}
