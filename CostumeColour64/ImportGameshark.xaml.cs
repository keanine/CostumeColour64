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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CostumeColour64
{
    /// <summary>
    /// Interaction logic for ImportGameshark.xaml
    /// </summary>
    public partial class ImportGameshark : Window
    {
        private MainWindow mainWindow;

        public ImportGameshark(MainWindow window)
        {
            InitializeComponent();
            mainWindow = window;
        }

        private void Import_Click(object sender, RoutedEventArgs e)
        {
            //loop through code, find and store info into structs
            string gameshark = CodeBox.Text;
            string codeType = gameshark.Substring(0, 2);
            string address = gameshark.Substring(2, 6);
            string value1 = gameshark.Substring(9, 2);
            string value2 = gameshark.Substring(11, 2);

            GamesharkCode code = new GamesharkCode(codeType, address, value1, value2);
            CodeBox.Text = code.address.ToString();

            //loop through list of structs, apply codes to byte list

            //Refresh the window
        }
    }

    public struct GamesharkCode
    {
        public string codeType;
        public int address;
        public int value1;
        public int value2;

        public GamesharkCode(string pCodeType, string pAddress, string pValue1, string pValue2)
        {
            codeType = pCodeType;
            address = int.Parse(pAddress, System.Globalization.NumberStyles.HexNumber);
            address -= int.Parse("245000", System.Globalization.NumberStyles.HexNumber);
            value1 = int.Parse(pValue1, System.Globalization.NumberStyles.HexNumber);
            value2 = int.Parse(pValue2, System.Globalization.NumberStyles.HexNumber);
        }
    }
}
