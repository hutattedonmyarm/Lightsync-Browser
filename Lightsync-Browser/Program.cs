using LedCSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Automation;

namespace Lightsync_Browser
{
    class Program
    {
        static void Main(string[] args)
        {
            LogitechGSDK.LogiLedInit();
            LogitechGSDK.LogiLedSaveCurrentLighting();
            Console.WriteLine("Grün");
            LogitechGSDK.LogiLedSetLighting(0, 100, 0);
            LogitechGSDK.LogiLedRestoreLighting();

            var automationElements = Browser.GetBrowserWindows(Firefox.ProcessName);

            var firefox = new Firefox(automationElements.First());
            //Console.WriteLine($"Firefox: {firefox.GetCurrentUrl()}");
            firefox.ColorChanged += ColorChanged;

            automationElements = Browser.GetBrowserWindows(Chrome.ProcessName);

            var chrome = new Chrome(automationElements.First());
            //Console.WriteLine($"Chrome: {chrome.GetCurrentUrl()}");
            chrome.ColorChanged += ColorChanged;

            Task.Delay(1000).Wait();
            Console.WriteLine("Rot Blinkend");
            LogitechGSDK.LogiLedFlashLighting(100, 0, 0, 3000, 500);
            Task.Delay(4000).Wait();
            Console.ReadKey();
            LogitechGSDK.LogiLedShutdown();
        }

        private static void ColorChanged(object sender, ColorChangedEventArgs e)
        {
            //Console.WriteLine($"Color changed to: {e.Color}");
            //var success = LogitechGSDK.LogiLedSaveCurrentLighting();
            //Console.WriteLine($"LogiLedSaveCurrentLighting: {success}");
            var success = LogitechGSDK.LogiLedSetLighting(e.Color.Red, e.Color.Green, e.Color.Blue);
            Console.WriteLine($"LogiLedSetLighting: {success}");
            //success = LogitechGSDK.LogiLedRestoreLighting();
            //Console.WriteLine($"LogiLedRestoreLighting: {success}");
        }
    }
}
