using mexLib.Attributes;
using mexLib.Installer;
using mexLib.MexScubber;
using System.ComponentModel;

namespace mexLib
{
    public class MexMusic
    {
        [Category("General"), DisplayName("Name"), Description("The name label used in-game to refer to the track")]
        public string Name { get; set; } = "";

        [Category("General"), DisplayName("Filename"), Description("Name of the file in audio folder")]
        [MexFilePathValidator(MexFilePathType.Audio)]
        public string FileName { get; set; } = "";

        public void FromDOL(MexDOL dol, uint index)
        {
            FileName = dol.GetStruct<string>(0x803BC314, index);
            Name = MexDefaultData.Music_Info[index].Item2;
            //Series = MexDefaultData.Music_Info[index].Item1;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
