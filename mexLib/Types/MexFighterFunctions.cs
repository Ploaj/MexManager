using HSDRaw.MEX;
using mexLib.Attributes;
using mexLib.MexScubber;
using System.ComponentModel;

namespace mexLib.Types
{
    public partial class MexFighter
    {
        [Browsable(false)]
        public FighterFunctions Functions { get; set; } = new FighterFunctions();

        public class FighterFunctions
        {
            [Browsable(false)]
            public int Version { get; set; } = 0;

            [Category("Fighter"), Description("Function to initialize unique fighter attribtues and items")]
            [DisplayHex]
            public uint OnLoad { get; set; }

            [Category("Fighter")]
            [DisplayHex]
            public uint OnRespawn { get; set; }

            [Category("Fighter")]
            [DisplayHex]
            public uint OnDestroy { get; set; }

            [Category("Fighter")]
            [DisplayHex]
            public uint MoveLogicPointer { get; set; }

            [Category("Fighter")]
            [DisplayHex]
            public uint DemoMoveLogicPointer { get; set; }

            [Category("Fighter")]
            [DisplayHex]
            public uint SpecialN { get; set; }

            [Category("Fighter")]
            [DisplayHex]
            public uint SpecialNAir { get; set; }

            [Category("Fighter")]
            [DisplayHex]
            public uint SpecialHi { get; set; }

            [Category("Fighter")]
            [DisplayHex]
            public uint SpecialHiAir { get; set; }

            [Category("Fighter")]
            [DisplayHex]
            public uint SpecialLw { get; set; }

            [Category("Fighter")]
            [DisplayHex]
            public uint SpecialLwAir { get; set; }

            [Category("Fighter")]
            [DisplayHex]
            public uint SpecialS { get; set; }

            [Category("Fighter")]
            [DisplayHex]
            public uint SpecialSAir { get; set; }

            [Category("Fighter")]
            [DisplayHex]
            public uint OnSmashHi { get; set; }

            [Category("Fighter")]
            [DisplayHex]
            public uint OnSmashLw { get; set; }

            [Category("Fighter")]
            [DisplayHex]
            public uint OnSmashF { get; set; }

            [Category("Fighter")]
            [DisplayHex]
            public uint OnAbsorb { get; set; }

            [Category("Fighter")]
            [DisplayHex]
            public uint OnItemPickup { get; set; }

            [Category("Fighter")]
            [DisplayHex]
            public uint OnMakeItemInvisible { get; set; }

            [Category("Fighter")]
            [DisplayHex]
            public uint OnMakeItemVisible { get; set; }

            [Category("Fighter")]
            [DisplayHex]
            public uint OnItemDrop { get; set; }

            [Category("Fighter")]
            [DisplayHex]
            public uint OnItemCatch { get; set; }

            [Category("Fighter")]
            [DisplayHex]
            public uint OnUnknownItemRelated { get; set; }

            [Category("Fighter")]
            [DisplayHex]
            public uint OnApplyHeadItem { get; set; }

            [Category("Fighter")]
            [DisplayHex]
            public uint OnRemoveHeadItem { get; set; }

            [Category("Fighter")]
            [DisplayHex]
            public uint EyeTextureDamaged { get; set; }

            [Category("Fighter")]
            [DisplayHex]
            public uint EyeTextureNormal { get; set; }

            [Category("Fighter")]
            [DisplayHex]
            public uint OnFrame { get; set; }

            [Category("Fighter")]
            [DisplayHex]
            public uint OnActionStateChange { get; set; }

            [Category("Fighter")]
            [DisplayHex]
            public uint ResetAttribute { get; set; }

            [Category("Fighter")]
            [DisplayHex]
            public uint OnModelRender { get; set; }

            [DisplayHex]
            [Category("Fighter")]
            public uint OnShadowRender { get; set; }

            [Category("Fighter")]
            [DisplayHex]
            public uint OnUnknownMultijump { get; set; }

            [Category("Fighter")]
            [DisplayHex]
            public uint OnActionStateChangeWhileEyeTextureIsChanged1 { get; set; }

            [Category("Fighter")]
            [DisplayHex]
            public uint OnActionStateChangeWhileEyeTextureIsChanged2 { get; set; }

            [Category("Fighter")]
            [DisplayHex]
            public uint OnTwoEntryTable1 { get; set; }

            [Category("Fighter")]
            [DisplayHex]
            public uint OnTwoEntryTable2 { get; set; }

            [Category("Fighter")]
            [DisplayHex]
            public uint OnLanding { get; set; }

            [Category("Fighter")]
            [DisplayHex]
            public uint OnExtRstAnim { get; set; }

            [Category("Fighter")]
            [DisplayHex]
            public uint OnIndexExtRstAnim { get; set; }

            [Category("Fighter")]
            [DisplayHex]
            public uint EnterFloat { get; set; }

            [Category("Fighter")]
            [DisplayHex]
            public uint EnterDoubleJump { get; set; }

            [Category("Fighter")]
            [DisplayHex]
            public uint EnterTether { get; set; }

            [Category("Fighter"), Description("")]
            [DisplayHex]
            public uint OnIntroL { get; set; }

            [Category("Fighter"), Description("")]
            [DisplayHex]
            public uint OnIntroR { get; set; }

            [Category("Fighter"), Description("")]
            [DisplayHex]
            public uint OnCatch { get; set; }

            [Category("Fighter"), Description("")]
            [DisplayHex]
            public uint OnAppeal { get; set; }

            [Category("Fighter"), Description("")]
            [DisplayHex]
            public uint GetSwordTrail { get; set; }

            [DisplayName("N Special"), Category("Kirby"), Description("")]
            [DisplayHex]
            public uint KirbySpecialN { get; set; }

            [DisplayName("N Air Special"), Category("Kirby"), Description("")]
            [DisplayHex]
            public uint KirbySpecialNAir { get; set; }

            [DisplayName("OnSwallow"), Category("Kirby"), Description("")]
            [DisplayHex]
            public uint KirbyOnSwallow { get; set; }

            [DisplayName("OnLoseAbility"), Category("Kirby"), Description("")]
            [DisplayHex]
            public uint KirbyOnLoseAbility { get; set; }

            [DisplayName("OnHit"), Category("Kirby"), Description("")]
            [DisplayHex]
            public uint KirbyOnHit { get; set; }

            [DisplayName("OnDeath"), Category("Kirby"), Description("")]
            [DisplayHex]
            public uint KirbyOnDeath { get; set; }

            [DisplayName("OnItemInit"), Category("Kirby"), Description("")]
            [DisplayHex]
            public uint KirbyOnItemInit { get; set; }

            [DisplayName("OnFrame"), Category("Kirby"), Description("")]
            [DisplayHex]
            public uint KirbyOnFrame { get; set; }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="mexData"></param>
            /// <param name="internalId"></param>
            public void ToMxDt(MEX_Data mexData, int internalId)
            {
                var ff = mexData.FighterFunctions;

                ff.MoveLogicPointers[internalId] = MoveLogicPointer;
                ff.OnLoad[internalId] = OnLoad;
                ff.OnDeath[internalId] = OnRespawn;
                ff.OnUnknown[internalId] = OnDestroy;
                ff.DemoMoveLogic[internalId] = DemoMoveLogicPointer;
                ff.SpecialN[internalId] = SpecialN;
                ff.SpecialNAir[internalId] = SpecialNAir;
                ff.SpecialHi[internalId] = SpecialHi;
                ff.SpecialHiAir[internalId] = SpecialHiAir;
                ff.SpecialS[internalId] = SpecialS;
                ff.SpecialSAir[internalId] = SpecialSAir;
                ff.SpecialLw[internalId] = SpecialLw;
                ff.SpecialLwAir[internalId] = SpecialLwAir;
                ff.OnAbsorb[internalId] = OnAbsorb;
                ff.onItemCatch[internalId] = OnItemPickup;
                ff.onMakeItemInvisible[internalId] = OnMakeItemInvisible;
                ff.onMakeItemVisible[internalId] = OnMakeItemVisible;
                ff.onItemPickup[internalId] = OnItemPickup;
                ff.onItemDrop[internalId] = OnItemDrop;
                ff.onItemCatch[internalId] = OnItemCatch;
                ff.onUnknownItemRelated[internalId] = OnUnknownItemRelated;
                ff.onApplyHeadItem[internalId] = OnApplyHeadItem;
                ff.onRemoveHeadItem[internalId] = OnRemoveHeadItem;
                ff.onHit[internalId] = EyeTextureDamaged;
                ff.onUnknownEyeTextureRelated[internalId] = EyeTextureNormal;
                ff.onFrame[internalId] = OnFrame;
                ff.onActionStateChange[internalId] = OnActionStateChange;
                ff.onRespawn[internalId] = ResetAttribute;
                ff.onModelRender[internalId] = OnModelRender;
                ff.onShadowRender[internalId] = OnShadowRender;
                ff.onUnknownMultijump[internalId] = OnUnknownMultijump;
                ff.onActionStateChangeWhileEyeTextureIsChanged[internalId * 2] = OnActionStateChangeWhileEyeTextureIsChanged1;
                ff.onActionStateChangeWhileEyeTextureIsChanged[internalId * 2 + 1] = OnActionStateChangeWhileEyeTextureIsChanged2;
                ff.onTwoEntryTable[internalId * 2] = OnTwoEntryTable1;
                ff.onTwoEntryTable[internalId * 2 + 1] = OnTwoEntryTable2;
                ff.onLand[internalId] = OnLanding;
                ff.onExtRstAnim[internalId] = OnExtRstAnim;
                ff.onIndexExtResultAnim[internalId] = OnIndexExtRstAnim;

                ff.onSmashDown[internalId] = OnSmashLw;
                ff.onSmashUp[internalId] = OnSmashHi;
                ff.onSmashForward[internalId] = OnSmashF;
                ff.enterFloat[internalId] = EnterFloat;
                ff.enterSpecialDoubleJump[internalId] = EnterDoubleJump;
                ff.enterTether[internalId] = EnterTether;
                ff.onIntroL[internalId] = OnIntroL;
                ff.onIntroR[internalId] = OnIntroR;
                ff.onCatch[internalId] = OnCatch;
                ff.onAppeal[internalId] = OnAppeal;
                ff.getTrailData[internalId] = GetSwordTrail;

                var kff = mexData.KirbyFunctions;
                kff.KirbyOnHit[internalId] = KirbyOnHit;
                kff.KirbyOnItemInit[internalId] = KirbyOnItemInit;
                kff.OnAbilityLose[internalId] = KirbyOnLoseAbility;
                kff.OnAbilityGain[internalId] = KirbyOnSwallow;
                kff.KirbySpecialN[internalId] = KirbySpecialN;
                kff.KirbySpecialNAir[internalId] = KirbySpecialNAir;
                kff.KirbyOnFrame[internalId] = KirbyOnFrame;
                kff.KirbyOnDeath[internalId] = KirbyOnDeath;
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="dol"></param>
            /// <param name="index"></param>
            public void FromDOL(MexDOL dol, uint index)
            {
                if (index > 0x21)
                    return;

                // Functions
                OnLoad = dol.GetStruct<uint>(0x803C1154, index);
                OnRespawn = dol.GetStruct<uint>(0x803C11D8, index);
                OnDestroy = dol.GetStruct<uint>(0x803C125C, index);
                SpecialN = dol.GetStruct<uint>(0x803C167C, index);
                SpecialNAir = dol.GetStruct<uint>(0x803C15F8, index);
                SpecialS = dol.GetStruct<uint>(0x803C13E8, index);
                SpecialSAir = dol.GetStruct<uint>(0x803C1574, index);
                SpecialHi = dol.GetStruct<uint>(0x803C1784, index);
                SpecialHiAir = dol.GetStruct<uint>(0x803C146C, index);
                SpecialLw = dol.GetStruct<uint>(0x803C1700, index);
                SpecialLwAir = dol.GetStruct<uint>(0x803C14F0, index);
                OnAbsorb = dol.GetStruct<uint>(0x803C1808, index);
                OnItemPickup = dol.GetStruct<uint>(0x803C188C, index);
                OnMakeItemInvisible = dol.GetStruct<uint>(0x803C1910, index);
                OnMakeItemVisible = dol.GetStruct<uint>(0x803C1994, index);
                OnItemDrop = dol.GetStruct<uint>(0x803C1A18, index);
                OnItemCatch = dol.GetStruct<uint>(0x803C1A9C, index);
                OnUnknownItemRelated = dol.GetStruct<uint>(0x803C1B20, index);
                OnApplyHeadItem = dol.GetStruct<uint>(0x803C1BA4, index);
                OnRemoveHeadItem = dol.GetStruct<uint>(0x803C1C28, index);
                EyeTextureDamaged = dol.GetStruct<uint>(0x803C1CAC, index);
                EyeTextureNormal = dol.GetStruct<uint>(0x803C1D30, index);
                OnFrame = dol.GetStruct<uint>(0x803C1DB4, index);
                OnActionStateChange = dol.GetStruct<uint>(0x803C1E38, index);
                ResetAttribute = dol.GetStruct<uint>(0x803C1EBC, index);
                OnModelRender = dol.GetStruct<uint>(0x803C20CC, index);
                OnShadowRender = dol.GetStruct<uint>(0x803C2150, index);
                OnUnknownMultijump = dol.GetStruct<uint>(0x803C21D4, index);
                OnActionStateChangeWhileEyeTextureIsChanged1 = dol.GetStruct<uint>(0x803C2258, index, stride: 8);
                OnActionStateChangeWhileEyeTextureIsChanged2 = dol.GetStruct<uint>(0x803C2258 + 0x04, index, stride: 8);
                OnTwoEntryTable1 = dol.GetStruct<uint>(0x803C25F4, index, stride: 8);
                OnTwoEntryTable2 = dol.GetStruct<uint>(0x803C25F4 + 0x04, index, stride: 8);
                OnExtRstAnim = dol.GetStruct<uint>(0x803C24EC, index);
                OnIndexExtRstAnim = dol.GetStruct<uint>(0x803C2570, index);

                KirbyOnSwallow = dol.GetStruct<uint>(0x803C9CC8 + 0x00, index, 8);
                KirbyOnLoseAbility = dol.GetStruct<uint>(0x803C9CC8 + 0x04, index, 8);
                KirbySpecialN = dol.GetStruct<uint>(0x803C9DD0, index);
                KirbySpecialNAir = dol.GetStruct<uint>(0x803C9E54, index);

                MoveLogicPointer = dol.GetStruct<uint>(0x803C12E0, index);
                DemoMoveLogicPointer = dol.GetStruct<uint>(0x803C1364, index);

                // special double jump code
                EnterDoubleJump = index switch
                {
                    0x08 => 0x800cbd18, // Ness
                    0x0E => 0x800cbe98, // Yoshi
                    0x09 => 0x800cc0e8, // Peach
                    0x10 => 0x800cc238, // Mewtwo
                    _ => 0x800cbbc0,    // Default
                };
            }

            internal void FromMxDt(MexWorkspace workspace, MEX_Data mxdt, int internalId)
            {
                var ff = mxdt.FighterFunctions;

                MoveLogicPointer = ff.MoveLogicPointers[internalId];
                OnLoad = ff.OnLoad[internalId];
                OnRespawn = ff.OnDeath[internalId];
                OnDestroy = ff.OnUnknown[internalId];
                DemoMoveLogicPointer = ff.DemoMoveLogic[internalId];
                SpecialN = ff.SpecialN[internalId];
                SpecialNAir = ff.SpecialNAir[internalId];
                SpecialHi = ff.SpecialHi[internalId];
                SpecialHiAir = ff.SpecialHiAir[internalId];
                SpecialS = ff.SpecialS[internalId];
                SpecialSAir = ff.SpecialSAir[internalId];
                SpecialLw = ff.SpecialLw[internalId];
                SpecialLwAir = ff.SpecialLwAir[internalId];
                OnAbsorb = ff.OnAbsorb[internalId];
                OnItemPickup = ff.onItemCatch[internalId];
                OnMakeItemInvisible = ff.onMakeItemInvisible[internalId];
                OnMakeItemVisible = ff.onMakeItemVisible[internalId];
                OnItemPickup = ff.onItemPickup[internalId];
                OnItemDrop = ff.onItemDrop[internalId];
                OnItemCatch = ff.onItemCatch[internalId];
                OnUnknownItemRelated = ff.onUnknownItemRelated[internalId];
                OnApplyHeadItem = ff.onApplyHeadItem[internalId];
                OnRemoveHeadItem = ff.onRemoveHeadItem[internalId];
                EyeTextureDamaged = ff.onHit[internalId];
                EyeTextureNormal = ff.onUnknownEyeTextureRelated[internalId];
                OnFrame = ff.onFrame[internalId];
                OnActionStateChange = ff.onActionStateChange[internalId];
                ResetAttribute = ff.onRespawn[internalId];
                OnModelRender = ff.onModelRender[internalId];
                OnShadowRender = ff.onShadowRender[internalId];
                OnUnknownMultijump = ff.onUnknownMultijump[internalId];
                OnActionStateChangeWhileEyeTextureIsChanged1 = ff.onActionStateChangeWhileEyeTextureIsChanged[internalId * 2];
                OnActionStateChangeWhileEyeTextureIsChanged2 = ff.onActionStateChangeWhileEyeTextureIsChanged[internalId * 2 + 1];
                OnTwoEntryTable1 = ff.onTwoEntryTable[internalId * 2];
                OnTwoEntryTable2 = ff.onTwoEntryTable[internalId * 2 + 1];
                OnLanding = ff.onLand[internalId];
                OnExtRstAnim = ff.onExtRstAnim[internalId];
                OnIndexExtRstAnim = ff.onIndexExtResultAnim[internalId];

                OnSmashLw = ff.onSmashDown[internalId];
                OnSmashHi = ff.onSmashUp[internalId];
                OnSmashF = ff.onSmashForward[internalId];
                EnterFloat = ff.enterFloat[internalId];
                EnterDoubleJump = ff.enterSpecialDoubleJump[internalId];
                EnterTether = ff.enterTether[internalId];
                OnIntroL = ff.onIntroL[internalId];
                OnIntroR = ff.onIntroR[internalId];
                OnCatch = ff.onCatch[internalId];
                OnAppeal = ff.onAppeal[internalId];
                GetSwordTrail = ff.getTrailData[internalId];

                var kff = mxdt.KirbyFunctions;
                KirbyOnHit = kff.KirbyOnHit[internalId];
                KirbyOnItemInit = kff.KirbyOnItemInit[internalId];
                KirbyOnLoseAbility = kff.OnAbilityLose[internalId];
                KirbyOnSwallow = kff.OnAbilityGain[internalId];
                KirbySpecialN = kff.KirbySpecialN[internalId];
                KirbySpecialNAir = kff.KirbySpecialNAir[internalId];
                KirbyOnFrame = kff.KirbyOnFrame[internalId];
                KirbyOnDeath = kff.KirbyOnDeath[internalId];
            }
        }
    }
}
