using Microsoft.Win32;
using System;
using System.IO;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace KSDMProgrammer2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        private static extern int SetWindowLong32(HandleRef hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(HandleRef hWnd, int nIndex, IntPtr dwNewLong);

        public IntPtr myHWND;
        public const int GWL_STYLE = -16;

        private Exe ex;
        private static Bkg bk = new Bkg();
        private DispatcherTimer uiTick;
        private static bool ui_lockout = true;

        public SaveFileDialog dlg = new SaveFileDialog();

        public OpenFileDialog openFileDialog1 = new OpenFileDialog();
        public static string ui_richTextBox_Text; // richTextBox1.Text
        public static int ui_openFileDialog_FilterIndex; // openFileDialog1.FilterIndex
        public static string[] ui_comboBox1_ItemsSource; // comboBox1.ItemsSource
        public static int ui_comboBox1_SelectedIndex; // comboBox1.SelectedIndex
        public static EventHandler ScanComplete;
        public static EventHandler ScanBegin;

        private bool scanningLogs = false;
        private readonly string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ksdm-temp\\");
        private readonly string logFileName = "KSDMLogFile.log";


        public MainWindow()
        {
            uiTick = new DispatcherTimer();
            uiTick.Tick += new EventHandler(ui_tick);
            uiTick.Interval = new TimeSpan(0, 0, 1);
            uiTick.Start();
            DateTime date = DateTime.Today;



            dlg.FileName = "KSDM3-Debug-Log_" + date.ToString("MM-dd-yy");
            dlg.DefaultExt = ".log";
            dlg.Filter = "Text documents (.log)|*.log";

            openFileDialog1.Filter = "Binary files (*.hex)|*.hex|UF2 Files (*.uf2)|*.uf2";
            openFileDialog1.FileOk += new CancelEventHandler(openFileDialog1_FileOk);
            
            InitializeComponent();
            Scan();
        }

        private void ui_tick(object sender, EventArgs e)
        {
            if (scanningLogs)
            {
                if (File.Exists(path+logFileName)) {
                    if (dlg.ShowDialog() == true)
                    {
                        File.Copy(path+logFileName, dlg.FileName, true);
                        File.Delete(path+logFileName);
                        scanningLogs = false;
                        ui_richTextBox_Text += "\n\r Log saved...";
                    }
                }
            }
            richTextBox1.Text = ui_richTextBox_Text;
            openFileDialog1.FilterIndex = ui_openFileDialog_FilterIndex;
            if (!ui_lockout)
            {
                comboBox1.ItemsSource = ui_comboBox1_ItemsSource;
                if (comboBox1.Items.Count - 1 >= ui_comboBox1_SelectedIndex)
                {
                    comboBox1.SelectedIndex = ui_comboBox1_SelectedIndex;
                }
                ui_lockout = true;
            }
            richTextBox1.SelectionStart = richTextBox1.Text.Length;
            richTextBox1.ScrollToEnd();
        }

        private void Scan()
        {
            ScanComplete += new EventHandler(c_ScanComplete);
            ScanBegin += new EventHandler(c_ScanBegin);
            bk.Scan();
        }
        private void minimizeButton_Click(object sender, RoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }
        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Storyboard sb = FindResource("closeSB") as Storyboard;
            Storyboard.SetTarget(sb, this);
            sb.Completed += Sb_Completed;
            sb.Begin();
        }

        private void Sb_Completed(object sender, EventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }

        private void button_Click_1(object sender, RoutedEventArgs e)
        {
            _ = openFileDialog1.ShowDialog();
        }
        private void openFileDialog1_FileOk(object sender, CancelEventArgs e) 
        {
            Debug.WriteLine("fileok");
            flashBtn.IsEnabled = true;
            textBox1.Text = openFileDialog1.FileName;
            richTextBox1.Foreground = Brushes.GreenYellow;
            bool typever = false;
            if (bk.found)
            {
                Debug.WriteLine("found");
                if (KSDM3.cpu == "avr")
                {
                    if (Helper.getFileExtension(openFileDialog1.FileName) == "hex")
                    {
                        Debug.WriteLine("typever=true"); 
                        typever = true;
                    }
                }
                else if (KSDM3.cpu == "rp2040")
                {
                    if (Helper.getFileExtension(openFileDialog1.FileName) == "uf2")
                    {
                        Debug.WriteLine("typever=true");
                        typever = true;
                    }
                }

                if (Exe.SubTypeCheck(openFileDialog1.FileName, KSDM3.submodel) && typever)
                    ui_richTextBox_Text += "\n\r" + "File verified! Ready to flash...";
                else
                    ui_richTextBox_Text += "\n\r" + "Warning: File NOT verified! Ready to flash...";
            }
            else
                ui_richTextBox_Text += "\n\r" + "Warning: File NOT verified! Ready to flash...";
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            ui_richTextBox_Text += "\n\r" + "Flashing device, please wait...";
            Thread.Sleep(50);
            ex = new Exe(comboBox1.Text, textBox1.Text);
            flashBtn.IsEnabled = false;
            WaitLoop();
        }
        private void WaitLoop()
        {
            while (true)
            {
                if (!ex.success)                                        // TODO: Add fail detection based on AVRDUDE output.
                {
                    richTextBox1.Foreground = Brushes.Red;
                    ui_richTextBox_Text += "\n\r" + "Failed to flash KSDM3, contact support@stinger.store";

                    return;
                }
                else if (ex.done)
                {
                    bool isRP = Helper.getFileExtension(openFileDialog1.FileName) == "uf2";
                    textBox1.Text = "";
                    openFileDialog1.FileName = "";
                    if (isRP)
                        ui_richTextBox_Text += "\n\r" + "Finished!";
                    else
                        ui_richTextBox_Text += "\n\r" + ex.output;

                    return;
                }
                else
                {
                    continue;
                }
            }
        }
        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void ScanBtn_Click(object sender, RoutedEventArgs e)
        {
            Scan();
        }
        public static void c_ScanComplete(object sender, EventArgs e)
        {
            
            if (bk.found)
            {
                ui_richTextBox_Text += "\n\r" + "Found KSDM3-" + KSDM3.cpu + " at com port: " + KSDM3.port + ".";
            }
            else
            {
                ui_richTextBox_Text += "\n\r" + "KSDM could not be automatically found, manually select a port or contact support@stinger.store";
            }
            if (KSDM3.cpu == "rp2040")
                ui_openFileDialog_FilterIndex = 2;
            else
                ui_openFileDialog_FilterIndex = 1;


            ui_comboBox1_ItemsSource = bk.nameArray;
            ui_comboBox1_SelectedIndex = 0;

            int cindex = 0;

            foreach (string prt in bk.nameArray)
            {
                if (prt == KSDM3.port || cindex > bk.nameArray.Length)
                {
                    break;
                }
                cindex++;
            }
            ui_comboBox1_SelectedIndex = cindex;
            ui_lockout = false;
        }
        
        public static void c_ScanBegin(object sender, EventArgs e)
        {
            ui_richTextBox_Text = "Scanning Ports Please Wait...";
            ui_lockout = false;
        }
        

        private void textBox1_GotFocus(object sender, RoutedEventArgs e)
        {
            textBox1.Text = "";
        }
        #region Windows handlers for default animation.
        public static class WS
        {
            public static readonly long
            WS_BORDER = 0x00800000L,
            WS_CAPTION = 0x00C00000L,
            WS_CHILD = 0x40000000L,
            WS_CHILDWINDOW = 0x40000000L,
            WS_CLIPCHILDREN = 0x02000000L,
            WS_CLIPSIBLINGS = 0x04000000L,
            WS_DISABLED = 0x08000000L,
            WS_DLGFRAME = 0x00400000L,
            WS_GROUP = 0x00020000L,
            WS_HSCROLL = 0x00100000L,
            WS_ICONIC = 0x20000000L,
            WS_MAXIMIZE = 0x01000000L,
            WS_MAXIMIZEBOX = 0x00010000L,
            WS_MINIMIZE = 0x20000000L,
            WS_MINIMIZEBOX = 0x00020000L,
            WS_OVERLAPPED = 0x00000000L,
            WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,
            WS_POPUP = 0x80000000L,
            WS_POPUPWINDOW = WS_POPUP | WS_BORDER | WS_SYSMENU,
            WS_SIZEBOX = 0x00040000L,
            WS_SYSMENU = 0x00080000L,
            WS_TABSTOP = 0x00010000L,
            WS_THICKFRAME = 0x00040000L,
            WS_TILED = 0x00000000L,
            WS_TILEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,
            WS_VISIBLE = 0x10000000L,
            WS_VSCROLL = 0x00200000L;
        }

        public static IntPtr SetWindowLongPtr(HandleRef hWnd, int nIndex, IntPtr dwNewLong)
        {
            if (IntPtr.Size == 8)
            {
                return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
            }
            else
            {
                return new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            myHWND = new WindowInteropHelper(this).Handle;
            IntPtr myStyle = new IntPtr(WS.WS_CAPTION | WS.WS_CLIPCHILDREN | WS.WS_MINIMIZEBOX | WS.WS_MAXIMIZEBOX | WS.WS_SYSMENU | WS.WS_SIZEBOX);
            SetWindowLongPtr(new HandleRef(null, myHWND), GWL_STYLE, myStyle);
        }
        #endregion

        private void DBG_Click(object sender, RoutedEventArgs e)
        {
            if (bk.found && KSDM3.cpu == "rp2040")
            {
                ui_richTextBox_Text += "\n\r" + "Scanning device for logs please wait...";
                scanningLogs = true;
                Bkg.getDebugLog(comboBox1.Text);
            } 
            else
            {
                ui_richTextBox_Text += "\n\r" + "Cannot scan this device for logs...";
            }
        }
    }
}
