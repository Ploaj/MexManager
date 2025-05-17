using System.Text;

namespace mexLib.Utilties
{
    internal class ShiftJIS
    {
        private static readonly Encoding SHIFT_JIS = Encoding.GetEncoding("Shift_JIS");

        private static readonly Dictionary<char, char> CharToShiftChar = new()
            {
                { '+', '＋' },
                { '-', '－' },
                { '=', '＝' },
                { '?', '？' },
                { '!', '！' },
                { '@', '＠' },
                { '%', '％' },
                { '&', '＆' },
                { '$', '＄' },
                { ',', '，' },
                { '•', '・' },
                { ';', '；' },
                { ':', '：' },
                { '^', '\uff3e' },
                { '_', '\uff3f' },
                { '—', 'ー' },
                { '~', '～' },
                { '/', '／' },
                { '|', '｜' },
                { '\\', '＼' },
                { '"', '“' },
                { '(', '（' },
                { ')', '）' },
                { '[', '［' },
                { ']', '］' },
                { '{', '｛' },
                { '}', '｝' },
                { '<', '〈' },
                { '>', '〉' },
                { '¥', '￥' },
                { '#', '＃' },
                { '*', '＊' },
                { '\'', '’' }
            };

        public static string ToShiftJIS(string value)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            char[] array = value.ToCharArray();
            for (int i = 0; i < array.Length; i++)
            {
                if (CharToShiftChar.ContainsKey(array[i]))
                {
                    array[i] = CharToShiftChar[array[i]];
                }
            }

            byte[] data = SHIFT_JIS.GetBytes(new string(array));
            return SHIFT_JIS.GetString(data, 0, data.Length);
        }

    }
}
