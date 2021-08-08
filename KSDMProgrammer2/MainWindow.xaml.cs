using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace KSDMProgrammer2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {

        public OpenFileDialog openFileDialog1 = new OpenFileDialog();
        private exe ex;
        private static bkg bk = new bkg();

        DispatcherTimer uiTick;
        public static string ui_richTextBox_Text; // richTextBox1.Text
        public static int ui_openFileDialog_FilterIndex; // openFileDialog1.FilterIndex
        public static string[] ui_comboBox1_ItemsSource; // comboBox1.ItemsSource
        public static int ui_comboBox1_SelectedIndex; // comboBox1.SelectedIndex


        public MainWindow()
        {
            uiTick = new DispatcherTimer();
            uiTick.Tick += new EventHandler(ui_tick);
            uiTick.Interval = new TimeSpan(0, 0, 1);
            uiTick.Start();

            openFileDialog1.Filter = "Binary files (*.hex)|*.hex|UF2 Files (*.uf2)|*.uf2";
            openFileDialog1.FileOk += new CancelEventHandler(openFileDialog1_FileOk);
            
            InitializeComponent();
            scan();
        }

        private void ui_tick(object sender, EventArgs e)
        {
            comboBox1.ItemsSource = ui_comboBox1_ItemsSource;
            richTextBox1.Text = ui_richTextBox_Text;
            openFileDialog1.FilterIndex = ui_openFileDialog_FilterIndex;

            if (comboBox1.Items.Count - 1 >= ui_comboBox1_SelectedIndex)
            {
                comboBox1.SelectedIndex = ui_comboBox1_SelectedIndex;
            }

            richTextBox1.SelectionStart = richTextBox1.Text.Length;
            richTextBox1.ScrollToEnd();
        }

        private void scan()
        {
            ScanComplete = c_ScanComplete;
            ScanBegin = c_ScanBegin;
            bk.Scan();
        }
        private void minimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowStyle = WindowStyle.SingleBorderWindow;
            WindowState = WindowState.Minimized;
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
            Debug.WriteLine("clicked");
            Application.Current.Shutdown();

        }

        private void button_Click_1(object sender, RoutedEventArgs e)
        {
            openFileDialog1.ShowDialog();
        }
        private void openFileDialog1_FileOk(object sender, CancelEventArgs e) 
        {
            Debug.WriteLine("fileok");
            flashBtn.IsEnabled = true;
            textBox1.Text = openFileDialog1.FileName;
            richTextBox1.Foreground = Brushes.GreenYellow;
            ui_richTextBox_Text += "\n\r" + "Ready to flash...";
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            ui_richTextBox_Text += "\n\r" + "Flashing device, please wait...";
            Thread.Sleep(50);
            ex = new exe(comboBox1.Text, textBox1.Text);
            flashBtn.IsEnabled = false;
            loopy();
        }
        private void loopy()
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
                    bool isRP = textBox1.Text.Contains("uf2");
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
            this.WindowState = WindowState.Minimized;
        }

        private void ScanBtn_Click(object sender, RoutedEventArgs e)
        {
            scan();
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
                if (prt == bk.potential || cindex > bk.nameArray.Length)
                {
                    break;
                }
                cindex++;
            }
            ui_comboBox1_SelectedIndex = cindex;
        }
        
        public static void c_ScanBegin(object sender, EventArgs e)
        {
            ui_richTextBox_Text = "Scanning Ports Please Wait...";
            
        }
        public static EventHandler ScanComplete;
        public static EventHandler ScanBegin;
    }
}
