using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;

namespace Lightsync_Browser
{
    public abstract class Browser: IColorProvider
    {
        protected AutomationElement _element;
        protected int _addressBarPatternNumber;
        protected IEnumerable<Condition> _addressBarConditions;
        protected AutomationElement _addressBar;
        protected AutomationPropertyChangedEventHandler _propChangeHandler;
        protected static Dictionary<string, LightsyncColor> hostColor = new Dictionary<string, LightsyncColor>
        {
            {"reddit.", new LightsyncColor(100, 0, 0) },
            {"facebook.", new LightsyncColor(0, 0, 100) },
            {"factorio.", new LightsyncColor(100, 65, 0) },
            {"app.getpocket.com", new LightsyncColor(70, 0, 0) },
            {"beta.pnut.io", new LightsyncColor(30, 30, 30) },
            {"tildes.", new LightsyncColor(15, 21, 39) }
        };

        public delegate void UrlChanged(object sender, EventArgs e);
        public event EventHandler<ColorChangedEventArgs> ColorChanged;

        protected AutomationElement AddressBar {
            get 
            {
                if (_addressBar == null)
                {
                    _addressBar = GetUrlBar();
                }
                return _addressBar;
            }
        }
        public static string ProcessName { get; }

        public Browser(AutomationElement element) {
            _element = element;
        }
        
        public abstract string GetCurrentUrl();

        protected virtual string TryGetAddressBarValue()
        {
            try
            {
                return ((ValuePattern)AddressBar.GetCurrentPattern(AddressBar.GetSupportedPatterns()[_addressBarPatternNumber])).Current.Value;
            }
            catch
            {
                return null;
            }
        }

        protected AutomationElement GetUrlBar()
        {
            var element = _element;
            foreach (var condition in _addressBarConditions)
            {
                element = element.FindFirst(TreeScope.Descendants, condition);
            }
            return element;
        }

        public static IEnumerable<AutomationElement> GetBrowserWindows(string processName)
        {
            var processes = Process.GetProcessesByName(processName);
            return processes.Where(x => x.MainWindowHandle != IntPtr.Zero).Select(x => AutomationElement.FromHandle(x.MainWindowHandle));
        }

        protected void SubscribePropertyChange()
        {
            Automation.AddAutomationPropertyChangedEventHandler(AddressBar,
                TreeScope.Element,
                _propChangeHandler = new AutomationPropertyChangedEventHandler(OnPropertyChange),
                 AutomationProperty.LookupById(ValuePattern.ValueProperty.Id));
        }
        protected void OnPropertyChange(object src, AutomationPropertyChangedEventArgs e)
        {
            AutomationElement sourceElement = src as AutomationElement;
            if (e.Property == ValuePattern.ValueProperty)
            {
                var newVal = (string)e.NewValue;
                var color = GetColorForUrl(newVal);
                if (color != null) {
                    var eventArgs = new ColorChangedEventArgs(color.Value);
                    ColorChanged?.Invoke(this, eventArgs);
                }
            }
            else
            {
                // TODO: Handle other property-changed events.
            }
        }

        protected static LightsyncColor? GetColorForUrl(string url)
        {
            var r = new Random();
            try
            {
                var uri = new Uri(url);
                Console.WriteLine($"New URL with host: {uri.Host}");
                var key = hostColor.Keys.FirstOrDefault(x => uri.Host.Contains(x));
                if (key != null)
                {
                    return hostColor[key];
                }

            } catch
            {
                return null;
            }
            return null;
        }
    }

    public class Firefox: Browser
    {
        public new static string ProcessName { get => "firefox"; }

        public Firefox (AutomationElement element): base(element)
        {

            _addressBarPatternNumber = 1;
            _addressBarConditions = new List<Condition> {
                new PropertyCondition(AutomationElement.NameProperty, "Navigation Toolbar"),
                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.ComboBox),
                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit)
            };
            SubscribePropertyChange();
        }
        public override string GetCurrentUrl()
        {
            return TryGetAddressBarValue();
        }
    }

    public class Chrome : Browser
    {
        public new static string ProcessName { get => "chrome"; }

        public Chrome(AutomationElement element) : base(element)
        {

            _addressBarPatternNumber = 0;
            _addressBarConditions = new List<Condition> {
                new PropertyCondition(AutomationElement.NameProperty, "Google Chrome"),
                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit)
            };
            SubscribePropertyChange();
        }
        public override string GetCurrentUrl()
        {
            return TryGetAddressBarValue();
        }
    }
}
