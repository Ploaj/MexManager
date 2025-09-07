using HSDRaw;
using mexLib.HsdObjects;
using mexLib.Utilties;

namespace mexLib.Generators
{
    public class GenerateMexSeries
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ws"></param>
        /// <returns></returns>
        public static bool Compile(MexWorkspace ws)
        {
            string path = ws.GetFilePath("MxSr.dat");
            byte[] data = ws.FileManager.Get(path);

            HSDRawFile file;
            if (data == null || 
                data.Length == 0)
            {
                file = new HSDRawFile();
            }
            else
            {
                file = new(path);
            }

            file.CreateUpdateSymbol("series_table", GenerateSeriesNode(ws));

            using MemoryStream stream = new();
            file.Save(stream);
            ws.FileManager.Set(path, stream.ToArray());
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ws"></param>
        /// <returns></returns>
        private static HSDSeriesNode GenerateSeriesNode(MexWorkspace ws)
        {
            HSDSeriesNode node = new();

            // create series data
            int series_id = 0;
            foreach (var s in ws.Project.Series)
            {
                if (s.Playlist.Entries.Count > 0)
                {
                    node.Series.Add(new HSDSeries()
                    {
                        SeriesID = series_id,
                        SeriesName = s.Name,
                        Playlist = s.Playlist.ToMexPlaylist(),
                    });
                }
                series_id++;
            }
            node.SeriesCount = node.Series.Length;

            // create stage lookup
            int internal_id = 0;
            foreach (var s in ws.Project.Stages)
            {
                node.StageLookup.Add(new HSDSeriesLookup() 
                { 
                    ExternalID = (ushort)MexStageIDConverter.ToExternalID(internal_id),
                    SeriesID = (ushort)s.SeriesID,
                });
                internal_id++;
            }
            node.StageLookup.Add(new HSDSeriesLookup()
            {
                ExternalID = 0xFFFF,
                SeriesID = 0xFFFF,
            });

            return node;
        }

    }
}
