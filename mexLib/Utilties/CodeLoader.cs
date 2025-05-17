using HSDRaw;
using mexLib.Types;
using System.Text;
using System.Text.RegularExpressions;

namespace mexLib.Utilties
{
    public class CodeLoader
    {
        /// <summary>
        /// Extracts Codes from INI file
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static IEnumerable<MexCode> FromINI(byte[] data)
        {
            using MemoryStream s = new(data);
            using StreamReader r = new(s);

            MexCode? c = null;
            StringBuilder? src = null;

            while (!r.EndOfStream)
            {
                string? line = r.ReadLine();

                if (line == null)
                    break;

                if (line.StartsWith("$"))
                {
                    if (c != null &&
                        src != null &&
                        src.Length > 0)
                    {
                        c.Source = src.ToString();
                        yield return c;
                    }

                    c = new MexCode();
                    src = new StringBuilder();

                    string l = line[1..];

                    if (l.StartsWith("!"))
                    {
                        c.Enabled = true;
                        l = l[1..];
                    }
                    else
                    {
                        c.Enabled = false;
                    }

                    Match name = Regex.Match(l, @"(?<=\[).+?(?=\]\s*$)");

                    if (name.Success)
                    {
                        c.Name = l[0..(name.Groups[0].Index - 1)].Trim();
                        c.Creator = name.Value;
                    }
                    else
                    {
                        c.Name = l;
                        c.Creator = "";
                    }
                }
                else
                if (c != null)
                {
                    if (line.StartsWith("*"))
                    {
                        c.Description += line[1..].Trim() + Environment.NewLine;
                    }
                    else
                    {
                        if (Hex.TrimHexLine(line, out string hexline))
                        {
                            src?.AppendLine(line);
                        }
                    }
                }
            }

            if (c != null && src?.Length > 0)
            {
                c.Source = src.ToString();
                yield return c;
            }
        }

        /// <summary>
        /// Packs codes into INI file
        /// </summary>
        /// <returns></returns>
        public static byte[] ToINI(IEnumerable<MexCode> codes)
        {
            using MemoryStream s = new();
            using StreamWriter w = new(s) { AutoFlush = true };

            foreach (MexCode c in codes)
            {
                w.WriteLine($"${(c.Enabled ? "!" : "")}{c.Name}{(string.IsNullOrEmpty(c.Creator) ? "" : $" [{c.Creator}]")}");

                string[] desc_lines = c.Description.Split(
                    new string[] { "\r\n", "\r", "\n" },
                    StringSplitOptions.None
                    );

                foreach (string l in desc_lines)
                {
                    if (string.IsNullOrEmpty(l))
                        continue;
                    w.WriteLine($"*{l}");
                }

                w.WriteLine(c.Source);
            }

            return s.ToArray();
        }


        /// <summary>
        /// Extracts Code from GCT file
        /// </summary>
        /// <param name="data"></param>
        /// <param name="codes"></param>
        /// <param name="is_gct"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public static MexCode? FromGCT(byte[] data)
        {
            using MemoryStream stream = new(data);
            using BinaryReaderExt r = new(stream)
            {
                BigEndian = true,
            };

            // check header
            if (r.ReadUInt32() != 0x00D0C0DE || r.ReadUInt32() != 0x00D0C0DE)
                return null;

            // create new code
            MexCode c = new()
            {
                Name = "gct",
            };

            // parse file
            c.SetCompiled(r.ReadBytes((int)(r.Length - r.Position - 8)));

            return c;
        }

        /// <summary>
        /// Packs given codes into a GCT file
        /// </summary>
        /// <param name="additional"></param>
        /// <returns></returns>
        public static byte[] ToGCT(IEnumerable<MexCode> codes)
        {
            using MemoryStream stream = new();
            using BinaryWriterExt r = new(stream)
            {
                BigEndian = true
            };

            // write header
            r.Write(0x00D0C0DE);
            r.Write(0x00D0C0DE);

            foreach (MexCode c in codes)
            {
                byte[]? comp = c.GetCompiled();
                if (comp != null)
                    r.Write(comp);
            }

            // write footer
            r.Write(0xF0000000);
            r.Write(0);

            return stream.ToArray();
        }
    }
}