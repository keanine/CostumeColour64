using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.IO;
using System.Windows.Media;
using Microsoft.Win32;

namespace CostumeColour64
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int red   = 0;
        private int green = 0;
        private int blue  = 0;

        public List<Mario64Color> mario64Colors = new List<Mario64Color>();
        public int mario64SelectedColor = 0;
        List<byte> offsetByte = new List<byte>();
        string fileDirectory = "";
        private int buttonCount = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        void LoadHex()
        {
            FileStream fs = new FileStream(fileDirectory, FileMode.Open);

            long offset = 8534896;
            long key = offset - (offset % 16);


            BinaryReader brFile = new BinaryReader(fs);
            fs.Position = key;
            offsetByte = brFile.ReadBytes(143).ToList();
            
            brFile.Close();
            fs.Close();
        }

        void DiscardChangesToColours()
        {
            Mario64Color mColor = mario64Colors[mario64SelectedColor];
            mColor.SetColor(offsetByte[mColor.hexIndex], offsetByte[mColor.hexIndex + 1], offsetByte[mColor.hexIndex + 2]);

            SelectColor(mario64SelectedColor, false);
        }

        void SaveHex()
        {
            mario64Colors[mario64SelectedColor].SetColor(red, green, blue);

            FileStream fs = new FileStream(fileDirectory, FileMode.Open);

            long offset = 8534896;

            for (int i = 0; i < mario64Colors.Count; i++)
            {

                long finalOffset = offset + mario64Colors[i].hexIndex;
                long key = finalOffset;
                fs.Position = key;
                byte r = (byte) mario64Colors[i].red;

                fs.WriteByte(r);
                fs.WriteByte((byte)mario64Colors[i].green);
                fs.WriteByte((byte)mario64Colors[i].blue);
            }
            fs.Close();
        }

        void AddColorsToList()
        {
            int i = 0;

            for (int x = 0; x < 36; x++)
            {
                mario64Colors.Add(new Mario64Color(i, offsetByte[i], offsetByte[i + 1], offsetByte[i + 2], NewButton()));
                i += 4;
            }
        }

        void ClearColors()
        {
            ColorPanel.Children.Clear();
        }

        Button NewButton()
        {
            Button b = new Button();
            b.Content = "";
            b.BorderThickness = new Thickness(0);
            b.Width = 24;
            b.Height = 24;
            b.Margin = new Thickness(2, 2, 0, 0);
            b.Tag = buttonCount;
            b.Click += new RoutedEventHandler(button_Click);
            ColorPanel.Children.Add(b);

            buttonCount++;
            return b;
        }

        void SelectColor(int selectedColor, bool savePrev = true)
        {
            if (savePrev) SaveCurrentColor();

            mario64Colors[mario64SelectedColor].button.BorderThickness = new Thickness(0);
            mario64SelectedColor = selectedColor;

            Mario64Color mColor = mario64Colors[mario64SelectedColor];
            mColor.button.BorderThickness = new Thickness(3);
            mColor.button.BorderBrush = new SolidColorBrush(Colors.DimGray);

            red =   mColor.red;
            green = mColor.green;
            blue =  mColor.blue;
            RedValue.Value = red;
            GreenValue.Value = green;
            BlueValue.Value = blue;
            RedText.Content = red.ToString("000");
            GreenText.Content = green.ToString("000");
            BlueText.Content = blue.ToString("000");
            Color c = Color.FromArgb(255, (byte)red, (byte)green, (byte)blue);
            SolidColorBrush brush = new SolidColorBrush(c);
            ColorPreview.Fill = brush;
        }

        void SaveCurrentColor()
        {
            mario64Colors[mario64SelectedColor].SetColor(red, green, blue);
        }

        private void RedValue_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!File.Exists(fileDirectory)) return;

            red = (int)RedValue.Value;
            RedText.Content = red.ToString("000");
            SetColorPreview();

        }

        private void GreenValue_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!File.Exists(fileDirectory)) return;

            green = (int)GreenValue.Value;
            GreenText.Content = green.ToString("000");
            SetColorPreview();
        }

        private void BlueValue_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!File.Exists(fileDirectory)) return;

            blue = (int)BlueValue.Value;
            BlueText.Content = blue.ToString("000");
            SetColorPreview();
        }

        void SetColorPreview()
        {
            Color c = Color.FromArgb(255, (byte)red, (byte)green, (byte)blue);
            SolidColorBrush brush = new SolidColorBrush(c);
            ColorPreview.Fill = brush;

            mario64Colors[mario64SelectedColor].button.Background = brush;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if(File.Exists(fileDirectory)) SaveHex();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            int myValue = (int)((Button)sender).Tag;
            SelectColor(myValue);
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            dialog.Filter = "N64 ROMs (*.z64)|*.z64";
            dialog.Title = "Open Mario 64 ROM";

            if (dialog.ShowDialog() == true)
            {
                fileDirectory = dialog.FileName;
                SelectedROM.Text = fileDirectory;
                LoadHex();
                ClearColors();
                AddColorsToList();

                SelectColor(0, false);
            }
        }

        private void Discard_OnClickscard_Click(object sender, RoutedEventArgs e)
        {
            DiscardChangesToColours();
        }
    }

    public class Mario64Color
    {
        public int red;
        public int green;
        public int blue;
        public int hexIndex;
        public Button button;

        public Mario64Color(int hexI, byte r, byte g, byte b, Button but)
        {
            
            red = r;
            green = g;
            blue = b;
            hexIndex = hexI;
            button = but;

            Color c = Color.FromArgb(255, (byte)red, (byte)green, (byte)blue);
            SolidColorBrush brush = new SolidColorBrush(c);
            button.Background = brush;
        }

        public void SetColor(int r, int g, int b)
        {
            red = r;
            green = g;
            blue = b;
        }
    }
}
