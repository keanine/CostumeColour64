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
using Newtonsoft.Json;

namespace CostumeColour64
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string defaultColours = "{\"bytes\":[0,0,127,0,0,0,127,0,0,0,255,0,0,0,255,0,40,40,40,0,0,0,0,0,127,0,0,0,127,0,0,0,255,0,0,0,255,0,0,0,40,40,40,0,0,0,0,0,127,127,127,0,127,127,127,0,255,255,255,0,255,255,255,0,40,40,40,0,0,0,0,0,57,14,7,0,57,14,7,0,114,28,14,0,114,28,14,0,40,40,40,0,0,0,0,0,127,96,60,0,127,96,60,0,254,193,121,0,254,193,121,0,40,40,40,0,0,0,0,0,57,3,0,0,57,3,0,0,115,6,0,0,115,6,0,0,40,40,40,0,0,0,0]}";
        
        private long key = 0;
        private bool overrideOffset = false;
        private long overrideValue = 0;

        private int red   = 0;
        private int green = 0;
        private int blue  = 0;

        public List<Mario64Color> mario64Colors = new List<Mario64Color>();
        public int mario64SelectedColor = 0;
        public List<byte> offsetByte = new List<byte>();
        string fileDirectory = "";
        string emuDirectory = "";
        private int buttonCount = 0;

        public MainWindow()
        {
            InitializeComponent();
            bool success = BrowseForFile();

            if (!success) Application.Current.Shutdown();
            emuDirectory = Properties.Settings.Default.EmuDirectory;
        }

        void LoadHex()
        {
            FileStream fs = new FileStream(fileDirectory, FileMode.Open);
            BinaryReader brFile = new BinaryReader(fs);

            long offset = 8534896 - 12; //Start from 12 behind incase the ROM is mapped differently
            if (overrideOffset)
            {
                offset = overrideValue;
                key = offset;
            }
            else
            {
                fs.Position = offset;
                while (brFile.PeekChar() == 01)
                {
                    brFile.ReadByte();
                }
                key = fs.Position;
            }

            offsetByte = brFile.ReadBytes(143).ToList();

            brFile.Close();
            fs.Close();
        }
        //void LoadHex()
        //{
        //    FileStream fs = new FileStream(fileDirectory, FileMode.Open);

        //    long offset = 8534896;
        //    long key = offset - (offset % 16);


        //    BinaryReader brFile = new BinaryReader(fs);
        //    fs.Position = key;
        //    offsetByte = brFile.ReadBytes(143).ToList();

        //    brFile.Close();
        //    fs.Close();
        //}

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

            for (int i = 0; i < mario64Colors.Count; i++)
            {
                long finalOffset = key + mario64Colors[i].hexIndex;
                fs.Position = finalOffset;
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
            Color c = Color.FromArgb(255, (byte) red, (byte) green, (byte) blue);
            SolidColorBrush brush = new SolidColorBrush(c);
            ColorPreview.Fill = brush;

            mario64Colors[mario64SelectedColor].button.Background = brush;

            if (!ColorHexInput.IsFocused)
            {
                ColorHexInput.Text = ("#" + red.ToString("x2") + green.ToString("x2") + blue.ToString("x2")).ToUpper();
            }

            if ((red + green + blue) / 3 < 128)
                ColorHexInput.Foreground = new SolidColorBrush(Colors.White);
            else
                ColorHexInput.Foreground = new SolidColorBrush(Colors.Black);
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
            BrowseForFile();
        }

        bool BrowseForFile()
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
                return true;
            }

            return false;
        }

        private void Discard_Click(object sender, RoutedEventArgs e)
        {
            DiscardChangesToColours();
        }

        private void SaveCollection()
        {
            SaveHex();
            LoadHex();

            Collection collection = new Collection();
            collection.bytes = offsetByte;
            string json = JsonConvert.SerializeObject(collection, Formatting.None);

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "CC64 Collection (*.cc64)|*.cc64";
            dialog.Title = "Save Costume Colour 64 Collection";

            if (dialog.ShowDialog() == true)
            {
                StreamWriter writer = new StreamWriter(dialog.FileName);
                writer.Write(json);
                writer.Close();
            }
        }

        private void LoadCollection()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            dialog.Filter = "CC64 Collection (*.cc64)|*.cc64";
            dialog.Title = "Open Costume Colour 64 Collection";

            if (dialog.ShowDialog() == true)
            {
                StreamReader reader = new StreamReader(dialog.FileName);
                string json = reader.ReadToEnd();
                reader.Close();

                Collection collection = JsonConvert.DeserializeObject<Collection>(json);
                offsetByte = collection.bytes;

                ClearColors();
                AddColorsToList();
            }
        }

        bool FindEmulator()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            dialog.Filter = "Emulator Executable (*.exe)|*.exe";
            dialog.Title = "Select Emulator Executable";

            if (dialog.ShowDialog() == true)
            {
                emuDirectory = dialog.FileName;
                Properties.Settings.Default.EmuDirectory = emuDirectory;
                Properties.Settings.Default.Save();
                return true;
            }

            return false;
        }

        private string colorHexBackup;

        private void ColorHexInput_GotFocus(object sender, RoutedEventArgs e)
        {
            colorHexBackup = ColorHexInput.Text;
        }

        private void ColorHexInput_LostFocus(object sender, RoutedEventArgs e)
        {
            FinishColorHexInput();
        }

        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                FinishColorHexInput();
            }
        }

        void FinishColorHexInput(bool adjustTextbox = true)
        {
            string hex;
            if (ColorHexInput.Text.StartsWith("#"))
            {
                hex = ColorHexInput.Text.Substring(1, ColorHexInput.Text.Length - 1);
            }
            else
            {
                hex = ColorHexInput.Text;
            }

            long output;
            if (long.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out output))
            {
                int zeroesToAdd = 6 - hex.Length;
                string finalHex = hex;
                for (int i = 0; i < zeroesToAdd; i++) finalHex += "0";
                if(adjustTextbox) ColorHexInput.Text = "#" + finalHex.ToUpper();

                string r = finalHex.Substring(1, 2);
                string g = finalHex.Substring(2, 2);
                string b = finalHex.Substring(4, 2);

                RedValue.Value = int.Parse(finalHex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                GreenValue.Value = int.Parse(finalHex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                BlueValue.Value = int.Parse(finalHex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            }
            else
            {
                if (adjustTextbox) ColorHexInput.Text = colorHexBackup;
            }
        }

        private void ColorHexInput_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            FinishColorHexInput(false);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------
        //-- MENU -----------------------------------------------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------------------------------------------------------

        private void menu_LoadROM_Click(object sender, RoutedEventArgs e)
        {
            BrowseForFile();
        }

        private void menu_SaveROM_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(fileDirectory)) 
            {
                SaveHex();
                MessageBox.Show("ROM saved!", "Attention", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
            }
        }

        private void menu_LaunchROM_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(emuDirectory))
            {
                MessageBoxResult r = MessageBox.Show("Emulator could not be found, please locate it to use the Launch feature", "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation, MessageBoxResult.OK);
                if (r != MessageBoxResult.OK) return;
                if (!FindEmulator()) return;
            }

            System.Diagnostics.Process.Start(emuDirectory, fileDirectory);
        }

        private void menu_ImportGameshark_Click(object sender, RoutedEventArgs e)
        {
            ImportGameshark importGameshark = new ImportGameshark(this);
            importGameshark.ShowDialog();
        }

        private void menu_LoadCollection_Click(object sender, RoutedEventArgs e)
        {
            LoadCollection();
        }

        private void menu_SaveCollection_Click(object sender, RoutedEventArgs e)
        {
            SaveCollection();
        }

        private void menu_LoadDefaultValues_Click(object sender, RoutedEventArgs e)
        {
            Collection collection = JsonConvert.DeserializeObject<Collection>(defaultColours);
            offsetByte = collection.bytes;

            ClearColors();
            AddColorsToList();
        }

        private void menu_DiscardAllChanges_Click(object sender, RoutedEventArgs e)
        {
            LoadHex();
            ClearColors();
            AddColorsToList();

            SelectColor(0, false);
        }

        private void menu_OverrideOffset_Click(object sender, RoutedEventArgs e)
        {
            overrideOffset = true;
        }

        private void Menu_ChangeEmu_OnClick_Click(object sender, RoutedEventArgs e)
        {
            FindEmulator();
        }
    }
}
