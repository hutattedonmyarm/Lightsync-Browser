using LedCSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Automation;
using System.Windows.Forms;

namespace Lightsync_Browser
{
    class LightsyncBrowserService
    {
        private NotifyIcon _icon;
        private Dictionary<IColorProvider, EventHandler<ColorChangedEventArgs>> _providers;

        static void Main(string[] args)
        {

            LogitechGSDK.LogiLedInit();
            _ = new LightsyncBrowserService();
            Application.Run();
            Application.ApplicationExit += Application_ApplicationExit;
        }

        private static void Application_ApplicationExit(object sender, EventArgs e)
        {
            LogitechGSDK.LogiLedShutdown();
        }

        private void TurnOff(object sender, EventArgs e)
        {
            Debug.WriteLine("Turning off");
            _icon.ContextMenuStrip.Items[0].Enabled = false;
            _icon.ContextMenuStrip.Items[1].Enabled = true;
            _icon.Text = "Off";
            UnsubscribeFromProviders();
        }

        private void TurnOn(object sender, EventArgs e)
        {
            Debug.WriteLine("Turning on");
            _icon.ContextMenuStrip.Items[0].Enabled = true;
            _icon.ContextMenuStrip.Items[1].Enabled = false;
            _icon.Text = "On";
            SubscribeToProviders();
        }

        private void Quit(object sender, EventArgs e)
        {
            Application.Exit();
        }

        public LightsyncBrowserService()
        {
            _providers = new Dictionary<IColorProvider, EventHandler<ColorChangedEventArgs>>();

            var menuStrip = new ContextMenuStrip();
            menuStrip.Items.Add("Deactivate", null, TurnOff);
            menuStrip.Items.Add(
            new ToolStripMenuItem("Activate", null, TurnOn)
            {
                Enabled = false
            });
            menuStrip.Items.Add("Quit", null, Quit);
            _icon = new NotifyIcon(new ControlContainer())
            {
                Text = "On",
                Icon = new System.Drawing.Icon("Assets/tray.ico"),
                ContextMenuStrip = menuStrip,
                Visible = true
            };

            SubscribeToBrowserColor(x => new Firefox(x), ColorChanged);
            SubscribeToBrowserColor(x => new Chrome(x), ColorChanged);
            SubscribeToBrowserColor(x => new Vivaldi(x), ColorChanged);
        }

        private void UnsubscribeFromProviders() {
            foreach (var provider in _providers)
            {
                provider.Key.ColorChanged -= provider.Value;
            }
            LogitechGSDK.LogiLedShutdown();
        }

        private void SubscribeToProviders()
        {
            LogitechGSDK.LogiLedInit();
            foreach (var provider in _providers)
            {
                provider.Key.ColorChanged += provider.Value;
            }
        }

        private void SubscribeToBrowserColor<T>(Func<AutomationElement, T> Constructor, EventHandler<ColorChangedEventArgs> handler) where T : Browser {
            var processName = (string)typeof(T).GetProperty("ProcessName").GetValue(null);
            var automationElements = Browser.GetBrowserWindows(processName);
            foreach (var window in automationElements)
            {
                var browser = Constructor(window);
                _providers.Add(browser, handler);
                browser.ColorChanged += handler;
            }
        }

        private static void ColorChanged(object sender, ColorChangedEventArgs e)
        {
            var success = LogitechGSDK.LogiLedSetLighting(e.Color.Red, e.Color.Green, e.Color.Blue);
            Debug.WriteLine($"LogiLedSetLighting success: {success}");
        }
    }

    class ControlContainer : IContainer
    {
        public ComponentCollection Components { get; private set; }

        public ControlContainer()
        {
            Components = new ComponentCollection(new IComponent[] { });
        }

        public void Add(IComponent component)
        {
        }

        public void Add(IComponent component, string name)
        {
        }

        public void Dispose()
        {
            Components = null;
        }

        public void Remove(IComponent component)
        {
        }
    }
}
