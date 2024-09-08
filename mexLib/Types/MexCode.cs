using mexLib.Utilties;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace mexLib.Types
{
    /// <summary>
    /// 
    /// </summary>
    public class MexCodeCompileError
    {
        public int LineIndex { get; set; }

        public string Description { get; set; }

        public MexCodeCompileError(int lineIndex, string description)
        {
            LineIndex = lineIndex;
            Description = description;
        }

        public override string ToString()
        {
            if (LineIndex != -1)
                return $"Error: {Description} on line {LineIndex}";
            else
                return $"Error: {Description}";
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class MexCode : MexReactiveObject
    {
        [Browsable(false)]
        public bool Enabled { get; set; } = true;

        public string Name { get; set; } = "";

        public string Creator { get; set; } = "";

        public string Description { get; set; } = "";

        [Browsable(false)]
        public string Source
        {
            get => _source;
            set
            {
                _source = value;
                CompileError = TryCompileCode();
                OnPropertyChanged();
            }
        }
        private string _source = "";

        private byte[]? _compiled;

        [Browsable(false)]
        [JsonIgnore]
        public MexCodeCompileError? CompileError
        {
            get => _compileError; internal set
            {
                _compileError = value;
                OnPropertyChanged();
            }
        }
        private MexCodeCompileError? _compileError;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public void SetCompiled(byte[] data)
        {
            _source = Hex.FormatByteArrayToHexLines(data);
            CompileError = TryCompileCode();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public byte[]? GetCompiled()
        {
            return _compiled;
        }
        /// <summary>
        /// Returns a list of used addresses
        /// </summary>
        /// <returns></returns>
        public IEnumerable<uint> UsedAddresses()
        {
            var code = GetCompiled();

            if (code == null)
                yield break;

            for (int i = 0; i < code.Length;)
            {
                switch (code[i])
                {
                    case 0x00:
                    case 0x02:
                    case 0x04:
                        {
                            yield return (uint)(0x80000000 | ((code[i + 1] & 0xFF) << 16 | (code[i + 2] & 0xFF) << 8 | code[i + 3] & 0xFF));
                            i += 8;
                        }
                        break;
                    case 0xC2:
                        {
                            yield return (uint)(0x80000000 | ((code[i + 1] & 0xFF) << 16 | (code[i + 2] & 0xFF) << 8 | code[i + 3] & 0xFF));
                            i += 4;
                            i += ((code[i] & 0xFF) << 24 | (code[i + 1] & 0xFF) << 16 | (code[i + 2] & 0xFF) << 8 | code[i + 3] & 0xFF) * 8 + 4;
                        }
                        break;
                    default:
                        throw new NotSupportedException($"Code type unknown/unsupported: 0x{code[i]:X2}");
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private MexCodeCompileError? TryCompileCode()
        {
            _compiled = null;

            if (_source == null)
                return new MexCodeCompileError(-1, "Error: No Source");

            var lines = _source.Split(
                new string[] { "\r\n", "\r", "\n" },
                StringSplitOptions.None
                );

            List<byte> data = new ();

            int line_index = 0;
            foreach (var l in lines)
            {
                if (string.IsNullOrEmpty(l))
                {
                    line_index++;
                    continue;
                }

                // remove spaces
                if (Hex.TrimHexLine(l, out string hexline))
                {
                    data.AddRange(Hex.StringToByteArray(hexline));
                }
                else
                {
                    return new MexCodeCompileError(line_index, "Error: Invalid HEX Format");
                }
                line_index++;
            }

            try
            {
                _compiled = CompressCode(data.ToArray());
                return null;
            }
            catch (Exception e)
            {
                _compiled = null;
                return new MexCodeCompileError(0, e.ToString());
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        private static byte[] CompressCode(byte[] code)
        {
            for (int i = 0; i < code.Length;)
            {
                switch (code[i])
                {
                    case 0x04:
                        {
                            i += 8;
                        }
                        break;
                    case 0xC2:
                        {
                            int start = i;
                            int count = (code[i + 4] & 0xFF) << 24 | (code[i + 5] & 0xFF) << 16 | (code[i + 6] & 0xFF) << 8 | code[i + 7] & 0xFF;
                            if (count == 1)
                            {
                                // compress this code
                                var comp = new byte[]
                                    {
                                        0x04, code[i + 1], code[i + 2], code[i + 3],
                                        code[i + 8], code[i + 9], code[i + 10], code[i + 11]
                                    };
                                code = ArrayExtensions.ReplaceRange(code, start, 8 + count * 8, comp);
                                i += 8;
                            }
                            else
                            {
                                i += 8 * (count + 1);
                            }
                        }
                        break;
                    default:
                        throw new NotSupportedException($"Code type unknown: 0x{code[i]:X2}");
                }
            }

            return code;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public MexCodeCompileError? TryCheckConflicts(MexCode code)
        {
            if (CompileError != null)
                return CompileError;

            var add1 = UsedAddresses();
            var add2 = code.UsedAddresses();

            foreach (var i in add1.Intersect(add2))
            {
                CompileError = new MexCodeCompileError(-1, $"Conflicting address {i:X8} with \"{code.Name}\"");
                return CompileError;
            }

            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name;
        }
    }
}
