using System.ComponentModel;
using System.Text.Json.Serialization;

namespace mexLib.Types
{
    public abstract class MexCodeBase : MexReactiveObject
    {
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }
        private string _name = "";

        [Browsable(false)]
        public bool Enabled { get; set; } = true;

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
        /// Returns a list of used addresses
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<uint> UsedAddresses();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public MexCodeCompileError? TryCheckConflicts(MexCodeBase code)
        {
            if (CompileError != null)
                return CompileError;

            IEnumerable<uint> add1 = UsedAddresses();
            IEnumerable<uint> add2 = code.UsedAddresses();

            foreach (uint i in add1.Intersect(add2))
            {
                CompileError = new MexCodeCompileError(-1, $"Conflicting address {i:X8} with \"{code.Name}\"");
                return CompileError;
            }

            return null;
        }
    }
}
