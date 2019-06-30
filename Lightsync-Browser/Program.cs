using LedCSharp;
using System;
using System.Threading.Tasks;
using System.Windows.Automation;

namespace Lightsync_Browser
{
    class Program
    {
        static void Main(string[] args)
        {
            LogitechGSDK.LogiLedInit();

            SubscribeToBrowserColor(x => new Firefox(x), ColorChanged);
            SubscribeToBrowserColor(x => new Chrome(x), ColorChanged);
            SubscribeToBrowserColor(x => new Vivaldi(x), ColorChanged);

            Console.ReadKey();
            LogitechGSDK.LogiLedShutdown();
        }

        private static void SubscribeToBrowserColor<T>(Func<AutomationElement, T> constructor, EventHandler<ColorChangedEventArgs> handler) where T : Browser {
            var processName = (string) typeof(T).GetProperty("ProcessName").GetValue(null);
            var automationElements = Browser.GetBrowserWindows(processName);
            foreach (var window in automationElements)
            {
                var browser = constructor(window);
                browser.ColorChanged += handler;
            }
        }

        private static void ColorChanged(object sender, ColorChangedEventArgs e)
        {
            var success = LogitechGSDK.LogiLedSetLighting(e.Color.Red, e.Color.Green, e.Color.Blue);
            Console.WriteLine($"LogiLedSetLighting success: {success}");
        }
    }
}
