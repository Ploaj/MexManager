using HSDRaw.MEX.Menus;
using mexLib.Attributes;
using System.ComponentModel;

namespace mexLib.Types
{
    public class MexCharacterSelectIcon : MexIconBase
    {
        public override float BaseWidth => 3.5f;

        public override float BaseHeight => 3.4f;

        [MexLink(MexLinkType.Fighter)]
        public int Fighter { get => _fighter; set { _fighter = value; OnPropertyChanged(); } }
        private int _fighter;

        [Browsable(false)]
        public int SFXID { get; set; }

        private float _collisionOffsetX = 0.0f;
        public float CollisionOffsetX { get => _collisionOffsetX; set { _collisionOffsetX = value; OnPropertyChanged(); } }

        private float _collisionOffsetY = 0.0f;
        public float CollisionOffsetY { get => _collisionOffsetY; set { _collisionOffsetY = value; OnPropertyChanged(); } }

        private float _collisionSizeX = 7.05f; //6.8f;
        public float CollisionSizeX { get => _collisionSizeX; set { _collisionSizeX = value; OnPropertyChanged(); } }

        private float _collisionSizeY = 7.2f; //7.0f;
        public float CollisionSizeY { get => _collisionSizeY; set { _collisionSizeY = value; OnPropertyChanged(); } }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
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
        public override int ImageKey => Fighter;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ws"></param>
        /// <returns></returns>
        public override MexImage? GetIconImage(MexWorkspace ws)
        {
            var internalId = MexFighterIDConverter.ToInternalID(Fighter, ws.Project.Fighters.Count);
            var fighter = ws.Project.Fighters[internalId];
            return fighter.Assets.CSSIconAsset.GetTexFile(ws);
        }
    }

}
