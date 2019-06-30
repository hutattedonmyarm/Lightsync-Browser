using System.Collections.Generic;
using System.Linq;
namespace Lightsync_Browser.Helper
{
    public static class Extensions
    {
        public static string EnsureStartsWith(this string str, string prefix)
        {
            return str.StartsWith(prefix) ? str : prefix + str;
        }

        public static string EnsureProtocol(this string str, IEnumerable<string> protocols)
        {
            if (protocols.Any(x => str.StartsWith(x)))
            {
                return str;
            }
            return protocols.First() + str;
        }
    }
}
