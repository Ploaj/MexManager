using HSDRaw.MEX.Menus;
using mexLib.Attributes;
using System.ComponentModel;

namespace mexLib.Types
{
    public class MexCharacterSelectIcon : MexIconBase
    {
        public override float BaseWidth => 3.5f;

        public override float BaseHeight => 3.4f;

        [Category("0 - Fighter")]
        [MexLink(MexLinkType.Fighter)]
        public int Fighter { get => _fighter; set { _fighter = value; OnPropertyChanged(); } }
        private int _fighter;

        public int SFXID { get; set; } = 205;

        private float _collisionOffsetX = 0.0f;

        [Category("2 - Collision")]
        [Browsable(false)]
        public float CollisionOffsetX { get => _collisionOffsetX; set { _collisionOffsetX = value; OnPropertyChanged(); } }

        private float _collisionOffsetY = 0.0f;
        [Category("2 - Collision")]
        [Browsable(false)]
        public float CollisionOffsetY { get => _collisionOffsetY; set { _collisionOffsetY = value; OnPropertyChanged(); } }

        private float _collisionSizeX = 7.05f; //6.8f;
        [Category("2 - Collision")]
        [Browsable(false)]
        public float CollisionSizeX { get => _collisionSizeX; set { _collisionSizeX = value; OnPropertyChanged(); } }

        private float _collisionSizeY = 7.2f; //7.0f;
        [Category("2 - Collision")]
        [Browsable(false)]
        public float CollisionSizeY { get => _collisionSizeY; set { _collisionSizeY = value; OnPropertyChanged(); } }
        public override (float, float) CollisionOffset => (CollisionOffsetX, CollisionOffsetY);
        public override (float, float) CollisionSize => (CollisionSizeX / 2, CollisionSizeY / 2);

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
                StatusID = Status.UnlockedAndVisible,
                JointID = (byte)(index + 1),
                UnkID = (byte)(index + 1),

                X1 = (float)(X - CollisionSizeX / 2.0 * ScaleX + CollisionOffsetX),
                Y1 = (float)(Y - CollisionSizeY / 2.0 * ScaleY + CollisionOffsetY),

                X2 = (float)(X + CollisionSizeX / 2.0 * ScaleX + CollisionOffsetX),
                Y2 = (float)(Y + CollisionSizeY / 2.0 * ScaleY + CollisionOffsetY),
            };
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetIconHash(MexWorkspace ws)
        {
            int internalId = MexFighterIDConverter.ToInternalID(Fighter, ws.Project.Fighters.Count);
            MexFighter fighter = ws.Project.Fighters[internalId];
            return fighter.Assets.CSSIconAsset.GetHashCode();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ws"></param>
        /// <returns></returns>
        public override MexImage? GetIconImage(MexWorkspace ws)
        {
            int internalId = MexFighterIDConverter.ToInternalID(Fighter, ws.Project.Fighters.Count);
            MexFighter fighter = ws.Project.Fighters[internalId];
            return fighter.Assets.CSSIconAsset.GetTexFile(ws);
        }
    }

}
