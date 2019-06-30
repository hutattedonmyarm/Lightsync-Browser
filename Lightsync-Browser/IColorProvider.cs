using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lightsync_Browser
{
    interface IColorProvider
    {
        event EventHandler<ColorChangedEventArgs> ColorChanged;
    }

    public class ColorChangedEventArgs : EventArgs
    {
        public ColorChangedEventArgs(LightsyncColor c)
        {
            Color = c;
        }
        public LightsyncColor Color { get; }
    }

    public struct LightsyncColor
    {
        public int Red, Green, Blue;
        public LightsyncColor(int red, int green, int blue)
        {
            Red = ClipColorValue(red);
            Green = ClipColorValue(green);
            Blue = ClipColorValue(blue);
        }

        private static int ClipColorValue(int value, int min = 0, int max = 100)
        {
            return Math.Max(Math.Min(value, max), min);
        }

        public override string ToString()
        {
            return $"R: {Red}, G: {Green}, B: {Blue}";
        }
    }
}
