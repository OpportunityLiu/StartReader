using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StartReader.ExtensionProvider
{
    class Helpers
    {
        private static readonly Regex jsKVP = new Regex(@"(^|,|;|\s)(?<key>\w+)\s*=\s*(?<q>['""])(?<value>.*?)(?<!\\)\k<q>", RegexOptions.ExplicitCapture);

        public static IDictionary<string, string> ParseJsKvp(string jsContent)
        {
            return jsKVP.Matches(jsContent)
                   .OfType<Match>().ToDictionary(m => m.Groups["key"].Value, m => m.Groups["value"].Value);
        }
    }
}
