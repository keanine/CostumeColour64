using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace CostumeColour64
{
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
