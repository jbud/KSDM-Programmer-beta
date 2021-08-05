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

        private OpenFileDialog openFileDialog1 = new OpenFileDialog();
        private exe ex;
        private string potential;
        System.Windows.Threading.DispatcherTimer loadTimer;
        public MainWindow()
        {
            loadTimer = new System.Windows.Threading.DispatcherTimer();
            loadTimer.Tick += new EventHandler(loadTimer_tick);
            loadTimer.Interval = new TimeSpan(0, 0, 1);
            loadTimer.Start();
            openFileDialog1.Filter = "Binary files (*.hex)|*.hex|UF2 Files (*.uf2)|*.uf2";
            openFileDialog1.FileOk += new CancelEventHandler(openFileDialog1_FileOk);
            InitializeComponent();
        }

        private void loadTimer_tick(object sender, EventArgs e)
        {
            scan();
            loadTimer.Stop();
        }
        private void scan()
        {
            Debug.WriteLine("Begin Scan");
            richTextBox1.Text = "Scanning for KSDM3, please wait...";
            string[] nameArray;
            nameArray = System.IO.Ports.SerialPort.GetPortNames();      // get a list of available ports
            string typeFound = "";
            bool found = false;
            string tr;

            // convoluted way to move COM1 to end of list, usually it's not what we're looking for but sometimes it can be.
            tr = nameArray[0];
            nameArray[0] = nameArray[nameArray.Length - 1];
            nameArray[nameArray.Length - 1] = tr;

            foreach (string b in nameArray)
            {
                string temp = exe.serialPoke(b);
                if (temp.Contains("ksdm3"))
                {
                    found = true;
                    if (temp.Contains("avr"))
                    {
                        typeFound = "KSDM3-avr";
                        potential = b;
                        break;
                    }
                    else if (temp.Contains("rp2040"))
                    {
                        typeFound = "KSDM3-rp2040";
                        openFileDialog1.FilterIndex = 2;
                        potential = b;
                        break;
                    }
                }
                continue;
            }
            if (found)
            {
                richTextBox1.Text += "\n\r" + "Found " + typeFound + " at com port: " + potential + ".";
            }
            else
            {
                richTextBox1.Text += "\n\r" + "KSDM could not be automatically found, manually select a port or contact support@stinger.store";
            }
            richTextBox1.SelectionStart = richTextBox1.Text.Length;
            richTextBox1.ScrollToEnd();
            comboBox1.ItemsSource = nameArray;
            comboBox1.SelectedIndex = 0;
            int cindex = 0;

            foreach (string prt in comboBox1.Items)
            {
                if (prt == potential || cindex == comboBox1.Items.Count)
                {
                    break;
                }
                cindex++;
            }
            if (comboBox1.Items.Count - 1 >= cindex)
            {
                comboBox1.SelectedIndex = cindex;
            }
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
            richTextBox1.Text += "\n\r" + "Ready to flash...";
            richTextBox1.SelectionStart = richTextBox1.Text.Length;
            richTextBox1.ScrollToEnd();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            richTextBox1.Text += "\n\r" + "Flashing device, please wait...";
            richTextBox1.SelectionStart = richTextBox1.Text.Length;
            richTextBox1.ScrollToEnd();
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
                    richTextBox1.Text += "\n\r" + "Failed to flash KSDM3, contact support@stinger.store";
                    richTextBox1.SelectionStart = richTextBox1.Text.Length;
                    richTextBox1.ScrollToEnd();

                    return;
                }
                else if (ex.done)
                {
                    bool isRP = textBox1.Text.Contains("uf2");
                    textBox1.Text = "";
                    openFileDialog1.FileName = "";
                    if (isRP)
                        richTextBox1.Text += "\n\r" + "Finished!";
                    else
                        richTextBox1.Text += "\n\r" + ex.output;                      // show all output from AVRDUDE 
                    richTextBox1.SelectionStart = richTextBox1.Text.Length;
                    richTextBox1.ScrollToEnd();

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
    }
}
