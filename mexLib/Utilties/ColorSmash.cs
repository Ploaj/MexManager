using HSDRaw.GX;
using HSDRaw.Tools;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Runtime.CompilerServices;

namespace mexLib.Utilties
{
    /// <summary>
    /// Ported from https://github.com/PeterHatch/color-smash
    /// </summary>
    public class ColorSmash
    {
        public enum ColorType { Rgba8, Rgb5a3 }

        public static void Quantize(
            IEnumerable<MexImage> meximages,
            ColorType colorType,
            int numColors,
            bool verbose)
        {
            var images = OpenImages(meximages);
            var quantMap = GetQuantizationMap(images, colorType, numColors, verbose);

            var colorCombos = new HashSet<Rgba32[]>(quantMap.Values, new SequenceComparer<Rgba32>());

            if (verbose)
                Console.WriteLine($"{colorCombos.Count} color combinations in output images");

            var orderedCombos = OrderColorCombinations(colorCombos);
            var indexedMap = IndexQuantizationMap(quantMap, orderedCombos);

            int width = images[0].Width;
            int height = images[0].Height;
            var indexedData = CalculateIndexes(images, indexedMap).ToArray();
            var rgbaPalettes = CalculatePalettes(orderedCombos);

            // swizzle image data (width proper padding)
            indexedData = Swizzle(indexedData, width, height, 8, 4);

            // set new data
            var mimg = meximages.ToArray();
            for (int i = 0; i < mimg.Length; i++)
            {
                // set smashed image data
                mimg[i].ImageData = indexedData;
                mimg[i].PaletteData = EncodePalette(rgbaPalettes[i].ToArray(), mimg[i].TlutFormat);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pOut"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="palettes"></param>
        /// <returns></returns>
        private static byte[] Swizzle(byte[] pOut, int width, int height, int tx, int ty)
        {
            byte[] array = new byte[Shared.AddPadding(width, tx) * Shared.AddPadding(height, ty)];
            int nh = Shared.AddPadding(height, ty) / ty;
            int nw = Shared.AddPadding(width, tx) / tx;
            int i = 0;
            Dictionary<int, int> tlutRemap = new Dictionary<int, int>();
            for (int h = 0; h < nh; h++)
            {
                for (int w = 0; w < nw; w++)
                {
                    for (int y = 0; y < ty; y++)
                    {
                        for (int x = 0; x < tx; x++)
                        {
                            if (w * tx + x < width && 
                                h * ty + y < height)
                            {
                                array[i] = pOut[width * (h * ty + y) + w * tx + x];
                            }

                            i++;
                        }
                    }
                }
            }
            return array;
        }
        private static byte[] EncodePalette(byte[] palette, GXTlutFmt palFormat)
        {
            byte[] array = new byte[palette.Length / 2];
            for (int i = 0; i < palette.Length / 4; i++)
            {
                int r = palette[i * 4];
                int g = palette[i * 4 + 1];
                int b = palette[i * 4 + 2];
                int a = palette[i * 4 + 3];
                switch (palFormat)
                {
                    case GXTlutFmt.IA8:
                        {
                            ushort num5 = GXImageConverter.EncodeIA8(a, b, g, r);
                            array[i * 2] = (byte)((uint)(num5 >> 8) & 0xFFu);
                            array[i * 2 + 1] = (byte)(num5 & 0xFFu);
                            break;
                        }
                    case GXTlutFmt.RGB565:
                        {
                            ushort num4 = GXImageConverter.EncodeRGB565(r, g, b);
                            array[i * 2] = (byte)((uint)(num4 >> 8) & 0xFFu);
                            array[i * 2 + 1] = (byte)(num4 & 0xFFu);
                            break;
                        }
                    case GXTlutFmt.RGB5A3:
                        {
                            ushort num3 = GXImageConverter.EncodeRGBA3(a, b, g, r);
                            array[i * 2] = (byte)((uint)(num3 >> 8) & 0xFFu);
                            array[i * 2 + 1] = (byte)(num3 & 0xFFu);
                            break;
                        }
                }
            }

            return array;
        }

        private static List<Image<Rgba32>> OpenImages(IEnumerable<MexImage> images)
        {
            return images.Select(i =>
            {
                return Image.LoadPixelData<Rgba32>(i.GetRgba(), i.Width, i.Height);
            }).ToList();
        }

        //public static void Quantize(
        //    IEnumerable<string> inputPaths,
        //    IEnumerable<string> outputPaths,
        //    ColorType colorType,
        //    int numColors,
        //    bool verbose)
        //{
        //    var images = OpenImages(inputPaths);
        //    var quantMap = GetQuantizationMap(images, colorType, numColors, verbose);

        //    var colorCombos = new HashSet<Rgba32[]>(quantMap.Values, new SequenceComparer<Rgba32>());

        //    if (verbose)
        //        Console.WriteLine($"{colorCombos.Count} color combinations in output images");

        //    var orderedCombos = OrderColorCombinations(colorCombos);
        //    var indexedMap = IndexQuantizationMap(quantMap, orderedCombos);

        //    int width = images[0].Width;
        //    int height = images[0].Height;
        //    var indexedData = CalculateIndexes(images, indexedMap);

        //    var (rgbPalettes, alphaPalettes) = CalculatePalettes(orderedCombos);
        //    WritePNGs(outputPaths, indexedData, rgbPalettes, alphaPalettes, width, height);
        //}

        //private static List<Image<Rgba32>> OpenImages(IEnumerable<string> paths)
        //{
        //    return paths.Select(path => Image.Load<Rgba32>(path)).ToList();
        //}

        private static Dictionary<Rgba32[], Rgba32[]> GetQuantizationMap(
            List<Image<Rgba32>> images,
            ColorType colorType,
            int numColors,
            bool verbose)
        {
            var combos = GetColorCombinations(images);
            var grouped = KMeans.CollectGroups(combos.Select(e => e.ToArray()));

            if (verbose)
                Console.WriteLine($"{grouped.Count} color combinations in input images");

            return KMeans.Run(grouped, numColors, verbose);
        }

        private static List<List<Rgba32>> GetColorCombinations(List<Image<Rgba32>> images)
        {
            int width = images[0].Width;
            int height = images[0].Height;
            var combos = new List<List<Rgba32>>();

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    combos.Add(images.Select(img => img[x, y]).ToList());

            return combos;
        }

        private static List<Rgba32[]> OrderColorCombinations(HashSet<Rgba32[]> combos)
        {
            return combos.OrderBy(c => c.Distinct().Count())
                         .ThenBy(c => c.Sum(p => p.A))
                         .ThenBy(c => c.Sum(p => p.R + p.G + p.B))
                         .ToList();
        }

        private static Dictionary<Rgba32[], int> IndexQuantizationMap(
            Dictionary<Rgba32[], Rgba32[]> map,
            List<Rgba32[]> ordered)
        {
            var comboToIndex = ordered.Select((c, i) => (c, i))
                                       .ToDictionary(t => t.c, t => t.i, new SequenceComparer<Rgba32>());
            return map.ToDictionary(kv => kv.Key, kv => comboToIndex[kv.Value], new SequenceComparer<Rgba32>());
        }

        private static List<byte> CalculateIndexes(List<Image<Rgba32>> images, Dictionary<Rgba32[], int> map)
        {
            int width = images[0].Width;
            int height = images[0].Height;
            var indexes = new List<byte>(width * height);

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    var key = images.Select(img => img[x, y]).ToArray();
                    indexes.Add((byte)map[key]);
                }

            return indexes;
        }

        private static List<List<byte>> CalculatePalettes(List<Rgba32[]> combos)
        {
            int numImages = combos[0].Length;
            var rgba = Enumerable.Range(0, numImages).Select(_ => new List<byte>()).ToList();

            foreach (var combo in combos)
                for (int i = 0; i < combo.Length; i++)
                {
                    var p = combo[i];
                    rgba[i].AddRange(new[] { p.B, p.G, p.R, p.A });
                }

            return rgba;
        }

        //private static void WritePNGs(
        //    IEnumerable<string> outputPaths,
        //    List<byte> indexed,
        //    List<List<byte>> rgbPalettes,
        //    List<List<byte>> alphaPalettes,
        //    int width,
        //    int height)
        //{
        //    var paths = outputPaths.ToArray();
        //    for (int i = 0; i < paths.Length; i++)
        //    {
        //        byte[] rgba = new byte[4 * width * height];

        //        int o = 0;
        //        for (int h = 0; h < height; h++)
        //            for (int w = 0; w < width; w++)
        //            {
        //                var index = indexed[w + h * width];
        //                rgba[o++] = rgbPalettes[i][index * 3 + 0];
        //                rgba[o++] = rgbPalettes[i][index * 3 + 1];
        //                rgba[o++] = rgbPalettes[i][index * 3 + 2];
        //                rgba[o++] = alphaPalettes[i][index];
        //            }

        //        using (Image<Rgba32> image = Image.LoadPixelData<Rgba32>(rgba, width, height))
        //        {
        //            image.Save(paths[i]);
        //        }
        //    }
        //}

        public class SequenceComparer<T> : IEqualityComparer<T[]>
        {
            public bool Equals(T[] x, T[] y) => x.SequenceEqual(y);
            public int GetHashCode(T[] obj) => obj.Aggregate(17, (hash, item) => hash * 23 + item?.GetHashCode() ?? 0);
        }

        public static class KMeans
        {
            public static List<Grouped<Rgba32[]>> CollectGroups(IEnumerable<Rgba32[]> items)
            {
                var counts = new Dictionary<string, (Rgba32[], int)>();

                foreach (var item in items)
                {
                    string key = Hash(item);
                    if (!counts.ContainsKey(key))
                        counts[key] = (item, 0);
                    counts[key] = (counts[key].Item1, counts[key].Item2 + 1);
                }

                return counts.Select(kv => new Grouped<Rgba32[]>(kv.Value.Item1, kv.Value.Item2)).ToList();
            }

            public static (List<Rgba32[]> centers, List<List<Grouped<Rgba32[]>>> clusters)
                InitializeCenters(List<Grouped<Rgba32[]>> points, int k)
            {
                var centers = new List<Rgba32[]>();
                var mostCommon = points.OrderByDescending(p => p.Count).First().Data;
                centers.Add(mostCommon);

                var distancePerPoint = points.Select(p => NormalizedDistance(p.Data, centers[0])).ToArray();
                var clusterPerPoint = new int[points.Count];
                var distancePerCluster = new List<double>
                {
                    points.Select((p, i) => distancePerPoint[i] * p.Count).Sum()
                };

                while (centers.Count < k)
                {
                    int worst = distancePerCluster
                        .Select((d, i) => (d, i))
                        .OrderByDescending(t => t.d)
                        .First().i;

                    int farthestIndex = Enumerable.Range(0, points.Count)
                        .Where(i => clusterPerPoint[i] == worst)
                        .OrderByDescending(i => distancePerPoint[i])
                        .First();

                    var newCenter = points[farthestIndex].Data;

                    if (centers.Any(c => SequenceEqual(c, newCenter)))
                        Console.WriteLine("Created duplicate center");

                    centers.Add(newCenter);
                    int newCluster = centers.Count - 1;
                    distancePerCluster.Add(0);

                    for (int i = 0; i < points.Count; i++)
                    {
                        double newDist = NormalizedDistance(points[i].Data, newCenter);
                        if (newDist < distancePerPoint[i])
                        {
                            distancePerCluster[clusterPerPoint[i]] -= distancePerPoint[i] * points[i].Count;
                            clusterPerPoint[i] = newCluster;
                            distancePerPoint[i] = newDist;
                            distancePerCluster[newCluster] += newDist * points[i].Count;
                        }
                    }
                }

                var clusters = Enumerable.Range(0, k).Select(_ => new List<Grouped<Rgba32[]>>()).ToList();
                for (int i = 0; i < points.Count; i++)
                    clusters[clusterPerPoint[i]].Add(points[i]);

                return (centers, clusters);
            }

            public static Dictionary<Rgba32[], Rgba32[]> Run(List<Grouped<Rgba32[]>> grouped, int k, bool verbose)
            {
                var (centers, clusters) = InitializeCenters(grouped, k);
                var clusterAssignments = new int[grouped.Count];

                while (true)
                {
                    foreach (var cluster in clusters) cluster.Clear();

                    Parallel.For(0, grouped.Count, i =>
                    {
                        var g = grouped[i];
                        int bestIndex = 0;
                        double bestDist = double.MaxValue;
                        for (int c = 0; c < centers.Count; c++)
                        {
                            double dist = Distance(g.Data, centers[c], bestDist);
                            if (dist < bestDist)
                            {
                                bestDist = dist;
                                bestIndex = c;
                            }
                        }
                        clusterAssignments[i] = bestIndex;
                    });

                    for (int i = 0; i < grouped.Count; i++)
                        clusters[clusterAssignments[i]].Add(grouped[i]);

                    var newCenters = clusters.Select(c => Average(c)).ToList();
                    if (centers.SequenceEqual(newCenters, new SequenceComparer()))
                        break;

                    centers = newCenters;
                }

                var map = new Dictionary<Rgba32[], Rgba32[]>(new SequenceComparer());
                for (int i = 0; i < clusters.Count; i++)
                    foreach (var g in clusters[i])
                        map[g.Data] = centers[i];

                return map;
            }

            private static double NormalizedDistance(Rgba32[] a, Rgba32[] b)
            {
                double sum = 0;
                for (int i = 0; i < a.Length; i++)
                {
                    double dr = (a[i].R - b[i].R) / 255.0;
                    double dg = (a[i].G - b[i].G) / 255.0;
                    double db = (a[i].B - b[i].B) / 255.0;
                    double da = (a[i].A - b[i].A) / 255.0;
                    sum += dr * dr + dg * dg + db * db + da * da;
                }
                return sum;
            }

            private static bool SequenceEqual(Rgba32[] a, Rgba32[] b)
            {
                if (a.Length != b.Length) return false;
                for (int i = 0; i < a.Length; i++)
                    if (!a[i].Equals(b[i])) return false;
                return true;
            }

            private static Rgba32[] Average(List<Grouped<Rgba32[]>> group)
            {
                if (group.Count == 0) return Array.Empty<Rgba32>();

                int channels = group[0].Data.Length;
                var sums = new int[channels * 4];
                int total = group.Sum(g => g.Count);

                foreach (var g in group)
                {
                    for (int i = 0; i < channels; i++)
                    {
                        var px = g.Data[i];
                        sums[i * 4 + 0] += px.R * g.Count;
                        sums[i * 4 + 1] += px.G * g.Count;
                        sums[i * 4 + 2] += px.B * g.Count;
                        sums[i * 4 + 3] += px.A * g.Count;
                    }
                }

                var avg = new Rgba32[channels];
                for (int i = 0; i < channels; i++)
                {
                    avg[i] = new Rgba32(
                        (byte)(sums[i * 4 + 0] / total),
                        (byte)(sums[i * 4 + 1] / total),
                        (byte)(sums[i * 4 + 2] / total),
                        (byte)(sums[i * 4 + 3] / total));
                }

                return avg;
            }

            private static double Distance(Rgba32[] a, Rgba32[] b, double max = double.MaxValue)
            {
                double sum = 0;
                for (int i = 0; i < a.Length; i++)
                {
                    sum += Square(a[i].R - b[i].R);
                    sum += Square(a[i].G - b[i].G);
                    sum += Square(a[i].B - b[i].B);
                    sum += Square(a[i].A - b[i].A);
                    if (sum > max)
                        return sum;
                }
                return sum;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static double Square(int x) => x * x;

            private static string Hash(Rgba32[] arr)
            {
                unchecked
                {
                    int hash = 17;
                    foreach (var px in arr)
                        hash = hash * 31 + HashCode.Combine(px.R, px.G, px.B, px.A);
                    return hash.ToString();
                }
            }

            private class SequenceComparer : IEqualityComparer<Rgba32[]>
            {
                public bool Equals(Rgba32[] x, Rgba32[] y) => SequenceEqual(x, y);

                public int GetHashCode(Rgba32[] obj)
                {
                    unchecked
                    {
                        int hash = 17;
                        foreach (var px in obj)
                            hash = hash * 31 + HashCode.Combine(px.R, px.G, px.B, px.A);
                        return hash;
                    }
                }
            }
        }

        public class Grouped<T>
        {
            public T Data { get; }
            public int Count { get; }

            public Grouped(T data, int count)
            {
                Data = data;
                Count = count;
            }
        }
    }
}
