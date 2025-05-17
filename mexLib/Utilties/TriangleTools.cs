using HSDRaw.GX;

namespace mexLib.Utilties
{
    public static class TriangleTools
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static List<T> QuadToList<T>(List<T> input)
        {
            List<T> output = new();

            for (int i = 0; i < input.Count; i += 4)
            {
                output.Add(input[i]);
                output.Add(input[i + 1]);
                output.Add(input[i + 2]);

                output.Add(input[i + 2]);
                output.Add(input[i + 3]);
                output.Add(input[i]);
            }

            return output;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static List<GX_Vertex> StripToList(List<GX_Vertex> input)
        {
            List<GX_Vertex> output = new();

            for (int index = 2; index < input.Count; index++)
            {
                bool isEven = index % 2 != 1;

                GX_Vertex vert1 = input[index - 2];
                GX_Vertex vert2 = isEven ? input[index] : input[index - 1];
                GX_Vertex vert3 = isEven ? input[index - 1] : input[index];

                if (!vert1.POS.Equals(vert2.POS) && !vert2.POS.Equals(vert3.POS) && !vert3.POS.Equals(vert1.POS))
                {
                    output.Add(vert3);
                    output.Add(vert2);
                    output.Add(vert1);
                }
            }

            return output;
        }
    }
}
