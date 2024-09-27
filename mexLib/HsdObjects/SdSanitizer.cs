using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace mexLib.HsdObjects
{
    internal static class SdSanitizer
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Decode(string input, out uint color)
        {
            input = input.Replace('　', ' ');
            input = input.Replace("<BR>", "\n");
            input = input.Replace("<END>", "");
            input = input.Replace("<RESET>", "");
            input = input.Replace("</COLOR>", "");

            // Regex pattern to find <CHR, X> where X is a number
            {
                string pattern = @"<CHR,\s*(\d+)>";
                input = Regex.Replace(input, pattern, m => $"{{{m.Groups[1].Value}}}");
            }

            // Perform regex match
            color = 0;
            {
                string pattern = @"<COLOR,\s*(\d{1,3}),\s*(\d{1,3}),\s*(\d{1,3})>";
                Match match = Regex.Match(input, pattern);
                if (match.Success)
                {
                    int red = int.Parse(match.Groups[1].Value);
                    int green = int.Parse(match.Groups[2].Value);
                    int blue = int.Parse(match.Groups[3].Value);
                    color = (uint)((255 << 24) | ((red & 0xFF) << 16) | ((green & 0xFF) << 8) | ((blue & 0xFF)));

                    // Remove the matched color pattern from the input string
                    input = Regex.Replace(input, pattern, "");
                }
            }

            return input;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Encode(string input, uint color, bool reset)
        {
            input = input.Replace(' ', '　');
            input = input.Replace("\n", "<BR>");
            {
                // Regex pattern to find "{X}" where X is a number
                string pattern = @"\{(\d+)\}";
                input = Regex.Replace(input, pattern, m => $"<CHR, {m.Groups[1].Value}>");
            }
            if (color != 0)
            {
                input += "</COLOR>";
            }

            input += "<END>";

            if (color != 0)
            {
                input = $"<COLOR, {(color >> 16) & 0xFF}, {(color >> 8) & 0xFF}, {color & 0xFF}>" + input;
            }
            if (reset)
            {
                input = "<RESET>" + input;
            }
            return input;
        }
    }
}
