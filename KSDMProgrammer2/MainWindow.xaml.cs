using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
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

namespace KSDMProgrammer2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private OpenFileDialog openFileDialog1 = new OpenFileDialog();
        private exe ex;
        public MainWindow()
        {
            openFileDialog1.Filter = "Binary files (*.hex)|*.hex|UF2 Files (*.uf2)|*.uf2";
            openFileDialog1.FileOk += new CancelEventHandler(openFileDialog1_FileOk);
            InitializeComponent();
            string[] nameArray;
            nameArray = System.IO.Ports.SerialPort.GetPortNames();
            comboBox1.ItemsSource = nameArray;
            comboBox1.SelectedIndex = 0;
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
            textBox1.Text = openFileDialog1.FileName;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (textBox1.Text.Contains(".uf2") || textBox1.Text.Contains(".hex"))
            {
                ex = new exe(comboBox1.Text, textBox1.Text);                // spawn AVRDUDE
            }
            else
            {
                textBox1.Text = "Error, please select a hex file before continuing";
            }
        }
    }
}
