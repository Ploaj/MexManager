using MeleeMedia.Audio;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace mexLib.Types
{
    public class MexSound : MexReactiveObject
    {
        private string _name = "";
        [Category("General")]
        [DisplayName("Name")]
        public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }
        /// <summary>
        /// 
        /// </summary>
        [Category("General")]
        [DisplayName("Loop Sound")]
        [JsonIgnore]
        public bool Loop
        {
            get
            {
                if (DSP != null)
                    return DSP.LoopSound;

                return false;
            }
            set
            {
                if (DSP != null)
                {
                    DSP.LoopSound = value;
                    OnPropertyChanged();
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        [Category("General")]
        [DisplayName("Loop Point")]
        [JsonIgnore]
        public string? LoopPoint
        {
            get => DSP?.LoopPoint;
            set
            {
                if (DSP != null)
                {
                    DSP.LoopPoint = value;
                    OnPropertyChanged();
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        [Category("General")]
        [DisplayName("Type")]
        [JsonIgnore]
        public string? ChannelType
        {
            get => DSP?.ChannelType;
        }
        /// <summary>
        /// 
        /// </summary>
        [Category("General")]
        [DisplayName("Sample Rate")]
        [JsonIgnore]
        public int SampleRate
        {
            get => DSP == null ? 0 : DSP.Frequency;
        }
        /// <summary>
        /// 
        /// </summary>
        [Category("General")]
        [DisplayName("Total Length")]
        [JsonIgnore]
        public string Length
        {
            get => DSP == null ? "" : DSP.Length;
        }
        /// <summary>
        /// 
        /// </summary>
        private DSP? _dsp;
        [JsonIgnore]
        [Browsable(false)]
        public DSP? DSP { get => _dsp; set { _dsp = value; OnPropertyChanged(); } }
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
