using System;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Reflection;

namespace KSDMProgrammer2
{
    internal class Exe
    {
        private string port;
        private string input;
        private string avrdude;
        private List<string> tempFiles = new List<string>();
        private readonly string exePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"ksdm-temp\\");
        
        // public object stuff
        public bool success;
        public bool done;
        public string output;
        public static string response;

        private bool SpawnProc(string filename, string arguments, bool events, bool readFromProc = true)
        {
            Process t = new Process();
            t.StartInfo.FileName = filename;
            t.StartInfo.Arguments = arguments;
            t.StartInfo.CreateNoWindow = true;
            t.StartInfo.UseShellExecute = false;
            t.StartInfo.RedirectStandardOutput = true;
            t.StartInfo.RedirectStandardError = true;
            t.EnableRaisingEvents = events;
            if (events)
                t.Exited += new EventHandler(P_Exited);

            try
            {
                _ = t.Start();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
            Debug.WriteLine("process spawned!");
            if (readFromProc) 
            { 
                output = t.StandardError.ReadToEnd();
                Debug.WriteLine(output);
            }
            return true;
        }

        private bool FlashRP2040()
        {

            bool procStatus = SpawnProc("cmd.exe", "/K Mode " + port + " baud=1200", false, false);
            if (procStatus)
            {
                Thread.Sleep(2000);                            // wait for windows to discover the Drive

                DriveInfo[] drives = DriveInfo.GetDrives();
                string path = "";
                foreach (DriveInfo d in drives)
                {
                    if (d.VolumeLabel == "RPI-RP2")
                    {
                        path = d.Name;
                        break;
                    }
                }

                try
                {
                    File.Copy(input, path + "ksdm3.uf2");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    return false;
                }
                done = true;
                return true;
            }
            else 
                return false;
        }
        private bool FlashAVR()
        {
            ExtractIncludes();
            bool procStatus = SpawnProc(avrdude, " -c arduino -p m328p -P " + port + " -b 57600 -e -u -D -U flash:w:" + input + ":i", true, true);

            if (procStatus)
            {
                DeleteExtractedFiles();
                if (output.Contains("verified"))
                    return true;
                else
                    return false;
            }
            else
            {
                DeleteExtractedFiles();
                return false;
            }
        }
                
        private void P_Exited(object sender, EventArgs e)
        {
            done = true;
        }
        private void ExtractIncludes()
        {
            bool procStatus = SpawnProc("cmd.exe", "/K mkdir " + exePath, false, false);
            
            if (procStatus) { 
                Assembly asm = Assembly.GetExecutingAssembly();
                string[] resources = asm.GetManifestResourceNames();

                string strip = "KSDMProgrammer2.Includes."; //"KSDMProgrammer2.Includes.avrdude.conf"

                avrdude = exePath + "avrdude.exe";

                foreach (string r in resources)
                {
                    if (r.Contains("Includes"))
                    {
                        string filename = r.Replace(strip, "");

                        Stream stream = GetType().Assembly.GetManifestResourceStream(r);
                        byte[] bytes = new byte[(int)stream.Length];
                        stream.Read(bytes, 0, bytes.Length);
                        File.WriteAllBytes(exePath + filename, bytes);
                        tempFiles.Add(filename);
                    }
                }
            }
        }
        private void DeleteExtractedFiles()
        {
            foreach (string f in tempFiles)
            {
                File.Delete(exePath + f);
            }
        }
        public static string SerialPoke(string pport)
        {
            string r = "failed";
            
            SerialPort p = new SerialPort();
            try
            {
                p.PortName = pport;
                p.BaudRate = 115200;
                p.DataBits = 8;
                p.StopBits = StopBits.One;
                p.Parity = Parity.None;
                p.Handshake = Handshake.XOnXOff;
                p.RtsEnable = true; // RP2040 type KSDM3 require RTS/DTR = true, this reboots AVR type so we sleep below.
                p.DtrEnable = true;
                p.Open();
                Thread.Sleep(1500); // AVR type KSDM3 reboot with this serial config, but will still open the port, give a few seconds to boot.
                while (true)
                {
                    if (p.IsOpen)
                    {
                        p.WriteLine("id");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            int tries = 0;
            while(true)
            {
                Thread.Sleep(150);
                try
                {
                    if (p.IsOpen) {
                        response = "";
                        if (p.BytesToRead > 0)
                            response = p.ReadExisting();

                        Debug.WriteLine("Resp: " + response);
                        if (response.Contains("ksdm3"))
                        {
                            r = response;
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
                if (tries >= 20)
                    break;

                tries++;
            }
            p.Close();
            return r;
        }

        public static bool SubTypeCheck(string filename, string subtype)
        {
            string fileHash;
            fileHash = Helper.getFileExtension(filename) == "uf2" ? Helper.getStringBetween(filename, "--", ".uf2") : Helper.getStringBetween(filename, "--", ".hex");

            if (fileHash == null)
                return false;

            int decrypted = Helper.BaseNDecode(fileHash);
            
            switch (subtype)
            {
                case "3":
                    return KSDM3.submodels.Base == decrypted;
                case "sportplus":
                    return KSDM3.submodels.SportPlus == decrypted;
                default:
                    return false;
            }
        }
        public Exe(string p, string i)
        {
            port = p;
            input = i;
            success = Helper.getFileExtension(input) == "uf2" ? FlashRP2040() : FlashAVR();
        }
    }
}
