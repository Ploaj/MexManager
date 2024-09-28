using HSDRaw.MEX;
using HSDRaw.MEX.Stages;
using mexLib.MexScubber;
using mexLib.Types;
using mexLib.Utilties;

namespace mexLib.Installer
{
    /// <summary>
    /// This class contains custom information about the vanilla game that isn't present in the dol
    /// </summary>
    internal static class MexDefaultData
    {
        // externalId
        public static readonly string[] Fighter_Names =
        {
            "C. Falcon",
            "DK",
            "Fox",
            "Game and Watch",
            "Kirby",
            "Bowser",
            "Link",
            "Luigi",
            "Mario",
            "Marth",
            "Mewtwo",
            "Ness",
            "Peach",
            "Pikachu",
            "Ice Climbers",
            "Jigglypuff",
            "Samus",
            "Yoshi",
            "Zelda",
            "Sheik",
            "Falco",
            "Young Link",
            "Dr. Mario",
            "Roy",
            "Pichu",
            "Ganondorf",
            "Master Hand",
            "Wireframe Male",
            "Wireframe Female",
            "Giga Bowser",
            "Crazy Hand",
            "Sandbag",
            "Nana",
        };

        // externalId
        public static readonly byte[] Fighter_SeriesIDs =
        {
            0x00, 0x01, 0x02, 0x03, 0x05, 0x06, 0x0D, 0x06, 0x06, 0x07, 0x09, 0x08, 0x06, 0x09, 0x04, 0x09, 0x0A, 0x0C, 0x0D, 0x0D, 0x02, 0x0D, 0x06, 0x07, 0x09, 0x0D, 0x0B, 0x0B, 0x0B, 0x0B, 0x0B, 0x0B, 0x04, 0x00, 0x00, 0x00
        };

        // externalId
        public static readonly int[] Fighter_AnnouncerCalls =
        {
            0x0007C830, 0x0007C831, 0x0007C835, 0x0007C83A, 0x0007C83F, 0x0007C840, 0x0007C842, 0x0007C844, 0x0007C845, 0x0007C846, 0x0007C848, 0x0007C84A, 0x0007C84B, 0x0007C84D, 0x0007C83B, 0x0007C83D, 0x0007C84E, 0x0007C84F, 0x0007C851, 0x0007C850, 0x0007C834, 0x0007C843, 0x0007C832, 0x0007C83C, 0x0007C84C, 0x0007C836, 0x0007C849, 0x0007C833, 0x0007C833, 0x0007C838, 0x0007C849, 0x0007C848, 0x0007C83B
        };

        // externalId
        public static readonly ushort[] Fighter_TargetTestStages =
        {
            0x0022, 0x0024, 0x0027, 0x0038, 0x0029, 0x002A, 0x002B, 0x002C, 0x0021, 0x002D, 0x002E, 0x002F, 0x0030, 0x0032, 0x0028, 0x0033, 0x0034, 0x0036, 0x0037, 0x0035, 0x0036, 0x0023, 0x0025, 0x0039, 0x0031, 0x003A, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0028
        };

        // externalId
        public static readonly int[] Fighter_VictoryThemes =
        {
            0x00000011, 0x0000000D, 0x00000010, 0x0000000F, 0x00000014, 0x00000016, 0x00000015, 0x00000016, 0x00000016, 0x0000000E, 0x00000018, 0x00000017, 0x00000016, 0x00000018, 0x00000013, 0x00000018, 0x00000019, 0x0000001D, 0x00000015, 0x00000015, 0x00000010, 0x00000015, 0x00000016, 0x0000000E, 0x00000018, 0x00000015, 0x00000000, 0x00000000, 0x00000000, 0x00000016, 0x00000000, 0x00000000, 0x00000013
        };

        // internalId
        public static readonly byte[] Fighter_CanWallJump =
        {
            0x01, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x01, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
        };

        // externalId
        public static readonly uint[] Fighter_RaceToTheFinishTimes =
        {
            0x00000027, 0x0000002B, 0x00000028, 0x00000030, 0x00000035, 0x00000034, 0x00000032, 0x00000035, 0x00000031, 0x0000002D, 0x0000002B, 0x0000002F, 0x00000032, 0x0000002C, 0x00000036, 0x00000033, 0x00000030, 0x0000002A, 0x00000032, 0x00000032, 0x0000002C, 0x0000002E, 0x00000032, 0x0000002E, 0x00000029, 0x00000036, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000, 0x00000000
        };

        public static readonly (int, string)[] Music_Info =
        {
            (11, "All-Star Rest Area"),
            (7, "Fire Emblem (Meeting Theme)"),
            (11, "Balloon Fight"),
            (0, "Big Blue"),
            (6, "Princess Peach's Castle"),
            (11, "Continue Screen"),
            (2, "Corneria"),
            (6, "Doctor Mario (Fever)"),
            (11, "1-Player Mode Ending"),
            (11, "FamiDemo"),
            (11, "1-Player Stage Clear Fanfare 1"),
            (11, "1-Player Stage Clear Fanfare 2"),
            (11, "Bad Fanfare"),
            (1, "Donkey Kong Fanfare"),
            (7, "Fire Emblem Fanfare"),
            (3, "Game & Watch Fanfare"),
            (2, "Star Fox Fanfare"),
            (0, "F-Zero Fanfare"),
            (11, "Good Fanfare"),
            (4, "Ice Climbers Fanfare"),
            (5, "Kirby Fanfare"),
            (13, "Legend of Zelda Fanfare"),
            (6, "Super Mario Fanfare"),
            (8, "Earthbound Fanfare"),
            (9, "Pokemon Fanfare"),
            (10, "Metroid Fanfare"),
            (11, "Step Fanfare 1"),
            (11, "Step Fanfare 2"),
            (11, "Step Fanfare 3"),
            (12, "Yoshi Fanfare"),
            (3, "Flat Zone"),
            (8, "Fourside"),
            (11, "Game Over Jingle"),
            (1, "Kongo Jungle"),
            (13, "Great Bay"),
            (5, "Green Greens"),
            (11, "How to Play"),
            (11, "How to Play"),
            (11, "Multi-Man Melee"),
            (11, "Multi-Man Melee (Alt)"),
            (4, "Icicle Mountain"),
            (6, "Mushroom Kingdom"),
            (6, "Mushroom Kingdom 20 seconds"),
            (6, "Mushroom Kingdom (Alt)"),
            (6, "Mushroom Kingdom (Alt) 20 seconds"),
            (11, "Classic Mode Intro"),
            (11, "Adventure Mode Intro"),
            (6, "Hammer"),
            (6, "Super Star"),
            (5, "Fountain of Dreams"),
            (1, "Jungle Japes"),
            (10, "Brinstar Depths"),
            (11, "Menu"),
            (11, "Trophy Tussle"),
            (11, "Menu (Alternate)"),
            (11, "Mach Rider"),
            (0, "Mute City"),
            (1, "N64 Congo Jungle"),
            (5, "N64 Dream Land"),
            (12, "N64 Yoshi's Island"),
            (8, "Onett"),
            (8, "Pollyanna"),
            (11, "Opening"),
            (9, "Battle Theme"),
            (9, "Pokemon Stadium"),
            (9, "Poke Floats"),
            (6, "Rainbow Cruise"),
            (11, "Notices 1"),
            (11, "Notices 2"),
            (11, "Notices 3"),
            (11, "New Trophy"),
            (11, "Rare New Trophy"),
            (11, "Challenger Approaching"),
            (11, "Unused"),
            (13, "Saria's Song"),
            (13, "Temple"),
            (10, "Brinstar Escape Shaft"),
            (6, "Super Mario Bros. 3"),
            (11, "Final Destination"),
            (11, "Giga Bowser"),
            (11, "Metal Battle"),
            (11, "Battlefield"),
            (11, "Special Movie"),
            (11, "Targets!"),
            (2, "Venom"),
            (11, "Giant Bowser Defeat"),
            (11, "Enter Luigi"),
            (2, "Arwings Assist"),
            (10, "Brinstar Explodes"),
            (11, "Giant Bowser's Defeat (Alternative)"),
            (11, "Giga Bowser's Defeat"),
            (0, "Intro to Grand Prix"),
            (11, "Enter Giga Bowser"),
            (11, "Tournament Mode"),
            (11, "Tournament Mode Alt"),
            (12, "Yoshi's Island"),
            (12, "Yoshi's Story"),
            (10, "Brinstar")
        };

        public static readonly int BaseItemCount = 0xED;

        public static readonly (string, string)[] Stage_Names =
        {
            ("Null", ""),
            ("Test Stage", ""),
            ("Princess Peach's Castle", "Mushroom Kingdom"),
            ("Rainbow Cruise", "Mushroom Kingdom"),
            ("Kongo Jungle", "DK Island"),
            ("Jungle Japes", "DK Island"),
            ("Great Bay", "Termina"),
            ("Temple", "Hyrule"),
            ("Brinstar", "Planet Zebes"),
            ("Brinstar Depths", "Planet Zebes"),
            ("Yoshi's Story", "Yoshi's Island"),
            ("Yoshi's Island", "Yoshi's Island"),
            ("Fountain of Dreams", "Dream Land"),
            ("Green Greens", "Dream Land"),
            ("Corneria", "Lylat System"),
            ("Venom", "Lylat System"),
            ("Pokemon Stadium", "Kanto"),
            ("Poke Floats", "Kanto Skies"),
            ("Mute City", "F-Zero Grand Prix"),
            ("Big Blue", "F-Zero Grand Prix"),
            ("Onett", "Eagleland"),
            ("Fourside", "Eagleland"),
            ("Icicle Mountain", "Infinite Glacier"),
            ("Ice Top", ""),
            ("Kingdom", "Mushroom"),
            ("Kingdom II", "Mushroom"),
            ("Akaneia", ""),
            ("Flat Zone", "Superflat World"),
            ("Dream Land N64", "Past Stages"),
            ("Yoshi's Island N64", "Past Stages"),
            ("Kongo Jungle N64", "Past Stages"),
            ("Mushroom Kingdom (Adventure)", ""),
            ("Underground Maze", ""),
            ("Brinstar Escape Shaft", ""),
            ("F-Zero Grand Prix", ""),
            ("Test Stage", ""),
            ("Battlefield", "Special Stages"),
            ("Final Destination", "Special Stages"),
            ("Trophy Collector", ""),
            ("Race to the Finish", ""),
            ("Targets!Mario", ""),
            ("Targets!C. Falcon", ""),
            ("Targets!Young Link", ""),
            ("Targets!DK", ""),
            ("Targets!Dr. Mario", ""),
            ("Targets!Falco", ""),
            ("Targets!Fox", ""),
            ("Targets!Ice Climbers", ""),
            ("Targets!Kirby", ""),
            ("Targets!Bowser", ""),
            ("Targets!Link", ""),
            ("Targets!Luigi", ""),
            ("Targets!Marth", ""),
            ("Targets!Mewtwo", ""),
            ("Targets!Ness", ""),
            ("Targets!Peach", ""),
            ("Targets!Pichu", ""),
            ("Targets!Pikachu", ""),
            ("Targets!Jigglypuff", ""),
            ("Targets!Samus", ""),
            ("Targets!Sheik", ""),
            ("Targets!Yoshi", ""),
            ("Targets!Zelda", ""),
            ("Targets!Game and Watch", ""),
            ("Targets!Roy", ""),
            ("Targets!Ganon", ""),
            ("All-Star Rest Area", ""),
            ("Home-Run Stadium", ""),
            ("Trophy (Goomba)", ""),
            ("Trophy (Entei)", ""),
            ("Trophy (Majora's Mask)", ""),
        };

        public static readonly int[] Stage_Series =
        {
            11, 11, 6, 6, 1, 1, 13, 13, 10, 10, 12, 12, 5, 5, 2, 2, 9, 9, 0, 0, 8, 8, 4, 4, 6, 6, 7, 3, 5, 12, 1, 6, 11, 10, 0, 11, 16, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11
        };

        public static IEnumerable<MexSeries> GenerateDefaultSeries()
        {
            yield return new MexSeries() { Name = "F-Zero" };
            yield return new MexSeries() { Name = "Donkey Kong" };
            yield return new MexSeries() { Name = "Star Fox" };
            yield return new MexSeries() { Name = "Game & Watch" };
            yield return new MexSeries() { Name = "Ice Climber" };
            yield return new MexSeries() { Name = "Kirby" };
            yield return new MexSeries() { Name = "Super Mario" };
            yield return new MexSeries() { Name = "Fire Emblem" };
            yield return new MexSeries() { Name = "EarthBound" };
            yield return new MexSeries() { Name = "Pokémon" };
            yield return new MexSeries() { Name = "Metroid" };
            yield return new MexSeries() { Name = "Smash Bros." };
            yield return new MexSeries() { Name = "Yoshi" };
            yield return new MexSeries() { Name = "The Legend of Zelda" };
            yield return new MexSeries() { Name = "Master Hand" };
            yield return new MexSeries() { Name = "Crazy Hand" };
            yield return new MexSeries() { Name = "Special Stages" };
        }

        public readonly static byte[] SinglePlayerIcon = { 0x54, 0x45, 0x58, 0x00, 0x08, 0x02, 0x00, 0x00, 0x18, 0x00, 0x00, 0x00, 0x18, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00, 0x20, 0x01, 0x00, 0x00, 0x40, 0x01, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x12, 0x00, 0x00, 0x02, 0x22, 0x00, 0x00, 0x22, 0x22, 0x00, 0x01, 0x22, 0x23, 0x00, 0x02, 0x22, 0x33, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x12, 0x22, 0x22, 0x21, 0x22, 0x22, 0x22, 0x22, 0x22, 0x11, 0x11, 0x22, 0x13, 0x33, 0x33, 0x31, 0x33, 0x33, 0x33, 0x33, 0x33, 0x33, 0x33, 0x33, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x21, 0x00, 0x00, 0x00, 0x22, 0x20, 0x00, 0x00, 0x22, 0x22, 0x00, 0x00, 0x32, 0x22, 0x10, 0x00, 0x33, 0x22, 0x20, 0x00, 0x00, 0x12, 0x21, 0x33, 0x00, 0x22, 0x23, 0x33, 0x00, 0x22, 0x13, 0x33, 0x00, 0x22, 0x13, 0x33, 0x00, 0x22, 0x13, 0x33, 0x00, 0x22, 0x13, 0x33, 0x00, 0x22, 0x23, 0x33, 0x00, 0x12, 0x21, 0x33, 0x33, 0x33, 0x33, 0x33, 0x33, 0x33, 0x33, 0x33, 0x33, 0x33, 0x33, 0x33, 0x33, 0x33, 0x33, 0x33, 0x33, 0x33, 0x33, 0x33, 0x33, 0x33, 0x33, 0x33, 0x33, 0x33, 0x33, 0x33, 0x33, 0x33, 0x33, 0x33, 0x33, 0x12, 0x21, 0x00, 0x33, 0x32, 0x22, 0x00, 0x33, 0x31, 0x22, 0x00, 0x33, 0x31, 0x22, 0x00, 0x33, 0x31, 0x22, 0x00, 0x33, 0x31, 0x22, 0x00, 0x33, 0x32, 0x22, 0x00, 0x33, 0x12, 0x21, 0x00, 0x00, 0x02, 0x22, 0x33, 0x00, 0x01, 0x22, 0x23, 0x00, 0x00, 0x22, 0x22, 0x00, 0x00, 0x02, 0x22, 0x00, 0x00, 0x00, 0x12, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x33, 0x33, 0x33, 0x33, 0x33, 0x33, 0x33, 0x33, 0x13, 0x33, 0x33, 0x31, 0x22, 0x11, 0x11, 0x22, 0x22, 0x22, 0x22, 0x22, 0x12, 0x22, 0x22, 0x21, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x33, 0x22, 0x20, 0x00, 0x32, 0x22, 0x10, 0x00, 0x22, 0x22, 0x00, 0x00, 0x22, 0x20, 0x00, 0x00, 0x21, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x2E, 0x00, 0xF8, 0x00, 0x1E, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };

        public static IEnumerable<MEX_EffectFiles> GenerateDefaultMexEffectSlots(MexDOL dol)
        {
            for (uint i = 0; i < 0x33; i++)
            {
                yield return new MEX_EffectFiles()
                {
                    FileName = dol.GetStruct<string>(0x803c025c + 0x00, i, 0x0C),
                    Symbol = dol.GetStruct<string>(0x803c025c + 0x04, i, 0x0C),
                };
            }
        }

        public static IEnumerable<MEX_StageIDTable> GenerateDefaultStageIDs(MexDOL dol)
        {
            for (uint i = 0; i < 0x11E; i++)
            {
                yield return new MEX_StageIDTable()
                {
                    StageID = (int)dol.GetStruct<uint>(0x803E9960 + 0x00, i, 0x0C),
                    Unknown1 = (int)dol.GetStruct<uint>(0x803E9960 + 0x04, i, 0x0C),
                    Unknown2 = (int)dol.GetStruct<uint>(0x803E9960 + 0x08, i, 0x0C),
                };
            }
        }

        public readonly static string[] TrophyIconsFiles =
        {
            "TyQuesD.dat",
            "TyMycCmA.dat",
            "TyMycCmB.dat",
            "TyMycCmC.dat",
            "TyMycCmD.dat",
            "TyMycCmE.dat",
            "TyMycR1A.dat",
            "TyMycR1B.dat",
            "TyMycR1C.dat",
            "TyMycR1D.dat",
            "TyMycR1E.dat",
            "TyMycR2A.dat",
            "TyMycR2B.dat",
            "TyMycR2C.dat",
            "TyMycR2D.dat",
            "TyMycR2E.dat",
            "TyMapA.dat",
            "TyMapB.dat",
            "TyMapC.dat",
            "TyMapD.dat",
            "TyMapE.dat",
            "TySeriA.dat",
            "TySeriB.dat",
            "TySeriC.dat",
            "TySeriD.dat",
            "TySeriE.dat",
            "TyEtcA.dat",
            "TyEtcB.dat",
            "TyEtcC.dat",
            "TyEtcD.dat",
            "TyEtcE.dat",
            "TyPokeA.dat",
            "TyPokeB.dat",
            "TyPokeC.dat",
            "TyPokeD.dat",
            "TyPokeE.dat",
            "TyItemA.dat",
            "TyItemB.dat",
            "TyItemC.dat",
            "TyItemD.dat",
            "TyItemE.dat",
            "TyStandD.dat",
            "TyQuesD.dat",
        };
    }
}
