using HSDRaw;
using HSDRaw.MEX.Menus;
using mexLib.Attributes;
using mexLib.MexScubber;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Numerics;

namespace mexLib.Types
{
    public class MexCharacterSelectIcon
    {
        [MexLink(MexLinkType.Fighter)]
        public int Fighter { get; set; }

        public int SFXID { get; set; }

        public float X { get; set; } = 0;

        public float Y { get; set; } = 0;

        public float Z { get; set; } = 0;

        public float ScaleX { get; set; } = 1.0f;

        public float ScaleY { get; set; } = 1.0f;

        public float CollisionOffsetX { get; set; } = 0.0f;

        public float CollisionOffsetY { get; set; } = 0.0f;

        public float CollisionSizeX { get; set; } = 6.8f;

        public float CollisionSizeY { get; set; } = 7.0f;

        public MEX_CSSIcon ToIcon(int index)
        {
            return new MEX_CSSIcon()
            {
                ExternalCharID = (byte)Fighter,
                SFXID = (byte)SFXID,

                JointID = (byte)(index + 1),
                UnkID = (byte)(index + 1),

                X1 = X - CollisionSizeX / 2 * ScaleX + CollisionOffsetX,
                Y1 = Y - CollisionSizeX / 2 * ScaleY + CollisionOffsetY,

                X2 = X + CollisionSizeX / 2 * ScaleX + CollisionOffsetX,
                Y2 = Y + CollisionSizeX / 2 * ScaleY + CollisionOffsetY,
            };
        }
    }

    public class MexCharacterSelect
    {
        public float CharacterSelectHandScale { get; set; } = 1.0f;

        public float StageSelectCursorStartX { get; set; } = 0;

        public float StageSelectCursorStartY { get; set; } = -17;

        public float StageSelectCursorStartZ { get; set; } = 0;

        public ObservableCollection<MexCharacterSelectIcon> FighterIcons { get; set; } = new();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dol"></param>
        public void FromDOL(MexDOL dol)
        {
            // CSSIconData - 0x803F0A48 0x398
            MEX_IconData css = new()
            {
                _s = new HSDStruct(dol.GetData(0x803F0A48, 0x398))
            };
            // extract icon data
            foreach (var i in css.Icons)
            {
                FighterIcons.Add(new MexCharacterSelectIcon()
                {
                    Fighter = i.ExternalCharID,
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gen"></param>
        public void ToMxDt(MexGenerator gen)
        {
            var tb = gen.Data.MenuTable;

            tb.Parameters = new MEX_MenuParameters()
            {
                CSSHandScale = 1.0f,
                StageSelectCursorStartX = StageSelectCursorStartX,
                StageSelectCursorStartY = StageSelectCursorStartY,
                StageSelectCursorStartZ = StageSelectCursorStartZ,
            };

            tb.CSSIconData = new MEX_IconData()
            {
                Icons = FighterIcons.Select((e, i) => e.ToIcon(i)).ToArray()
            };
        }
    }
}
