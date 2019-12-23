using System;
using System.Text;

namespace Colibri.Helpers
{
    public static class StringFormatter
    {
        public static string HtmlDecode(string input)
        {
            var sb = new StringBuilder(System.Net.WebUtility.HtmlDecode(input));
            sb.Replace("<br>", Environment.NewLine);

            return sb.ToString();
        }
    }
}