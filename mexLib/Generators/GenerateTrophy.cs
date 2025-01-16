using HSDRaw.Melee;
using HSDRaw;
using HSDRaw.Melee.Ty;
using mexLib.HsdObjects;
using mexLib.Types;
using System.Collections.ObjectModel;

namespace mexLib.Generators
{
    public class GenerateTrophy
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ws"></param>
        public static void Compile(MexWorkspace ws)
        {
            var trophies = ws.Project.Trophies;
            if (trophies.Count == 0)
                return;

            var sdToyFile = new HSDRawFile(ws.GetFilePath("SdToy.usd"));
            var sdToyExpFile = new HSDRawFile(ws.GetFilePath("SdToyExp.usd"));

            var sdToy = sdToyFile.Roots[0].Data as SBM_SISData;
            var sdToyExp = sdToyExpFile.Roots[0].Data as SBM_SISData;

            if (sdToy == null || sdToyExp == null)
                return;

            var off = BuildSdToy(ws.Project.Trophies, sdToy, sdToyExp, out int sdOffset);
            if (sdToyExpFile["offset"] == null)
            {
                sdToyExpFile.Roots.Add(new HSDRootNode() { Name = "offset", Data = off });
            }
            else
            {
                sdToyExpFile["offset"].Data = off;
            }
            var sdToyOff = new HSDIntArray() { Array = new int[] { sdOffset } };
            if (sdToyFile["offset"] == null)
            {
                sdToyFile.Roots.Add(new HSDRootNode() { Name = "offset", Data = sdToyOff });
            }
            else
            {
                sdToyFile["offset"].Data = sdToyOff;
            }

            var tyDataf = BuildTyDataf(ws.Project.Trophies);

            HSDRawFile datai = new ();
            datai.Roots.Add(new HSDRootNode() { Name = "tyDisplayModelTbl", Data = BuildDisplay2DModelTable(trophies, false) });
            datai.Roots.Add(new HSDRootNode() { Name = "tyDisplayModelUsTbl", Data = BuildDisplay2DModelTable(trophies, true) });

            HSDShortArray diffexp = new ();
            diffexp._s.Resize(0);
            for (short i = 0; i < ws.Project.Trophies.Count; i++)
            {
                if (ws.Project.Trophies[i].HasUSData)
                    diffexp.Add(i);
            }
            diffexp.Add(-1);
            datai.Roots.Add(new HSDRootNode() { Name = "tyExpDifferentTbl", Data = diffexp });


            datai.Roots.Add(new HSDRootNode() { Name = "tyInitModelDTbl", Data = BuildDisplayModelTable(trophies, true) });
            datai.Roots.Add(new HSDRootNode() { Name = "tyInitModelTbl", Data = BuildDisplayModelTable(trophies, false) });

            datai.Roots.Add(new HSDRootNode() { Name = "tyModelSortTbl", Data = BuildSortTable(trophies) });

            HSDShortArray noget = new ();
            noget._s.Resize(0);
            for (short i = 0; i < ws.Project.Trophies.Count; i++)
                if (ws.Project.Trophies[i].JapanOnly)
                    noget.Add(i);
            noget.Add(-1);
            datai.Roots.Add(new HSDRootNode() { Name = "tyNoGetUsTbl", Data = noget });

            // save all files
            {
                using var stream = new MemoryStream();
                sdToyFile.Save(stream);
                ws.FileManager.Set(ws.GetFilePath("SdToy.usd"), stream.ToArray());
            }
            {
                using var stream = new MemoryStream();
                sdToyExpFile.Save(stream);
                ws.FileManager.Set(ws.GetFilePath("SdToyExp.usd"), stream.ToArray());
            }
            {
                using var stream = new MemoryStream();
                tyDataf.Save(stream);
                ws.FileManager.Set(ws.GetFilePath("TyDataf.dat"), stream.ToArray());
            }
            {
                using var stream = new MemoryStream();
                datai.Save(stream);
                ws.FileManager.Set(ws.GetFilePath("TyDatai.usd"), stream.ToArray());
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="trophies"></param>
        /// <returns></returns>
        private static HSDRawFile BuildTyDataf(IEnumerable<MexTrophy> trophies)
        {
            HSDArrayAccessor<SBM_TyModelFileEntry> files = new ();
            HSDArrayAccessor<SBM_TyModelFileEntry> filesAlt = new ();

            int index = 0;
            foreach (var v in trophies)
            {
                files.Add(new SBM_TyModelFileEntry()
                {
                    TrophyIndex = index,
                    FileName = v.Data.File.File,
                    SymbolName = v.Data.File.Symbol,
                });
                
                if (v.HasUSData)
                {
                    filesAlt.Add(new SBM_TyModelFileEntry()
                    {
                        TrophyIndex = index,
                        FileName = v.USData.File.File,
                        SymbolName = v.USData.File.Symbol,
                    });
                }
                index++;
            }

            var file = new HSDRawFile();
            file.Roots.Add(new HSDRootNode() { Name = "tyModelFileTbl", Data = files });
            file.Roots.Add(new HSDRootNode() { Name = "tyModelFileUsTbl", Data = filesAlt });
            return file;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="trophies"></param>
        /// <returns></returns>
        private static SISOffset BuildSdToy(IEnumerable<MexTrophy> trophies, SBM_SISData namesis, SBM_SISData expsis, out int sdOffset)
        {
            List<SIS_Data> names = new ()
            { 
                new SIS_Data() { TextCode = "Dummy<END>" } 
            };
            List<SIS_Data> namesUS = new()
            {
                new SIS_Data() { TextCode = "Dummy<END>" }
            };
            List<SIS_Data> desc = new ()
            {
                new SIS_Data() { TextCode = "<RESET>None<END>" }
            };
            List<SIS_Data> descUS = new ()
            {
                new SIS_Data() { TextCode = "<RESET>None<END>" }
            };
            List<SIS_Data> src1 = new ()
            {
                new SIS_Data() { TextCode = "<RESET>None<END>" }
            };
            List<SIS_Data> src1US = new ()
            {
                new SIS_Data() { TextCode = "<RESET>None<END>" }
            };
            List<SIS_Data> src2 = new ()
            {
                new SIS_Data() { TextCode = "<RESET>None<END>" }
            };
            List<SIS_Data> src2US = new ()
            {
                new SIS_Data() { TextCode = "<RESET>None<END>" }
            };

            int index = 0;
            foreach (var v in trophies)
            {
                // non us data
                {
                    var data = v.Data.Text;
                    names.Add(new SIS_Data() { TextCode = SdSanitizer.Encode(data.Name, 0, false) });
                    desc.Add(new SIS_Data() { TextCode = SdSanitizer.Encode(data.Description, data._descriptionColor, true) });
                    src1.Add(new SIS_Data() { TextCode = SdSanitizer.Encode(data.Source1, data._source1Color, true) });
                    src2.Add(new SIS_Data() { TextCode = SdSanitizer.Encode(data.Source2, data._source2Color, true) });
                }

                // us only data
                if (v.HasUSData)
                {
                    var data = v.USData.Text;
                    namesUS.Add(new SIS_Data() { TextCode = SdSanitizer.Encode(data.Name, 0, false) });
                    descUS.Add(new SIS_Data() { TextCode = SdSanitizer.Encode(data.Description, data._descriptionColor, true) });
                    src1US.Add(new SIS_Data() { TextCode = SdSanitizer.Encode(data.Source1, data._source1Color, true) });
                    src2US.Add(new SIS_Data() { TextCode = SdSanitizer.Encode(data.Source2, data._source2Color, true) });
                }
                index++;
            }

            // append us names to end
            names.AddRange(namesUS);

            // 
            sdOffset = names.Count + 2;

            // add additional texts
            // x12 of these v
            // <CHR, 1><CHR, 1><CHR, 1><END>
            // 0123456789　<END>
            // x<END>
            // The<S>class<S>of<S>trophies<S>you<S>can<S>collect<S>has<S>increased.<END>
            for (int i = 0; i < 12; i++)
                names.Add(new SIS_Data()
                {
                    TextCode = "<CHR, 1><CHR, 1><CHR, 1><END>"
                });
            names.Add(new SIS_Data() { TextCode = "0123456789　<END>" });
            names.Add(new SIS_Data() { TextCode = "x<END>" });
            names.Add(new SIS_Data() { TextCode = "The<S>class<S>of<S>trophies<S>you<S>can<S>collect<S>has<S>increased.<END>" });

            // dump to sis data
            namesis.SISData = names.ToArray();

            SISOffset off = new()
            {
                Desciption = 2,
                Src1 = desc.Count + 2,
            };

            // compile descriptions into same list
            desc.AddRange(src1);
            off.Src2 = desc.Count + 2;
            desc.AddRange(src2);
            off.DesciptionAlt = desc.Count + 2;
            desc.AddRange(descUS);
            off.Src1Alt = desc.Count + 2;
            desc.AddRange(src1US);
            off.Src2Alt = desc.Count + 2;
            desc.AddRange(src2US);

            // dump to sis data
            expsis.SISData = desc.ToArray();

            return off;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="trophies"></param>
        /// <param name="is_us"></param>
        /// <returns></returns>
        private static HSDArrayAccessor<SBM_tyDisplayModelEntry> BuildDisplay2DModelTable(ObservableCollection<MexTrophy> trophies, bool is_us)
        {
            var tbl = new HSDArrayAccessor<SBM_tyDisplayModelEntry>();

            for (int i = 0; i < trophies.Count; i++)
            {
                var param = is_us ? trophies[i].USData.Param2D : trophies[i].Data.Param2D;

                if (is_us && trophies[i].HasUSData || !is_us)
                    tbl.Add(new SBM_tyDisplayModelEntry()
                    {
                        TrophyID = i,
                        Trophy2DFileIndex = param.FileIndex,
                        Trophy2DImageIndex = param.ImageIndex,
                        OffsetX = param.OffsetX,
                        OffsetY = param.OffsetY,
                    });
            }

            tbl.Add(new SBM_tyDisplayModelEntry()
            {
                TrophyID = -1,
            });

            return tbl;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="trophies"></param>
        /// <param name="is_us"></param>
        /// <returns></returns>
        private static HSDArrayAccessor<SBM_tyInitModelEntry> BuildDisplayModelTable(ObservableCollection<MexTrophy> trophies, bool is_us)
        {
            var tbl = new HSDArrayAccessor<SBM_tyInitModelEntry>();

            for (int i = 0; i < trophies.Count; i++)
            {
                var param = is_us ? trophies[i].USData.Param3D : trophies[i].Data.Param3D;

                if (is_us && trophies[i].HasUSData || !is_us)
                    tbl.Add(new SBM_tyInitModelEntry()
                    {
                        TrophyID = i,
                        TrophyType = (int)param.TrophyType,
                        OffsetX = param.OffsetX,
                        OffsetY = param.OffsetY,
                        OffsetZ = param.OffsetZ,
                        ModelScale = param.TrophyScale,
                        StandScale = param.StandScale,
                        YRotation = param.Rotation,
                        x20 = (byte)param.UnlockCondition,
                        x21 = param.UnlockParam,
                    });
            }

            tbl.Add(new SBM_tyInitModelEntry()
            {
                TrophyID = -1,
                ModelScale = 1,
                StandScale = 1,
                x20 = 99,
                x21 = 1,
            });

            return tbl;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="trophies"></param>
        /// <returns></returns>
        private static HSDArrayAccessor<SBM_tyModelSortEntry> BuildSortTable(ObservableCollection<MexTrophy> trophies)
        {
            var tbl = new HSDArrayAccessor<SBM_tyModelSortEntry>();
            var list = trophies.ToList();
            var sorted = trophies
                .OrderBy(e => e.Data.Text.Name)
                .ThenBy(e => e.Data.Param3D.TrophyType)
                .ToList();
            var sortedUS = trophies
                .OrderBy(e => e.USData.Text != null ? e.USData.Text.Name : e.Data.Text.Name)
                .ThenBy(e => e.USData.Param3D != null ? e.USData.Param3D.TrophyType : e.Data.Param3D.TrophyType)
                .ToList();

            for (short i = 0; i < trophies.Count; i++)
            {
                tbl.Add(new SBM_tyModelSortEntry()
                {
                    TrophyID = i,
                    x02 = (short)list.FindIndex(e => e.SortSeries == i),
                    x04 = (short)sorted.IndexOf(trophies[i]), // list.FindIndex(e => e.SortAlphabeticalJ == i),
                    x06 = (short)list.IndexOf(sorted[i]),
                    x08 = (short)sortedUS.IndexOf(trophies[i]), // list.FindIndex(e => e.SortAlphabeticalJUS == i),
                    x0A = (short)list.IndexOf(sortedUS[i]),
                }); ;
            }

            tbl.Add(new SBM_tyModelSortEntry()
            {
                TrophyID = -1,
                x02 = -1,
            });

            return tbl;
        }
    }
}
