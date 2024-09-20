namespace mexLib
{
    public class MexStageIDConverter
    {
        private static readonly int[] ExternalToInternal = new int[]
        {
            0, 1, 12, 16, 2, 4, 8, 14, 10, 20, 18, 3, 5, 6, 7, 9, 11, 13, 21, 24, 25, 26, 15, 17, 19, 22, 22, 27, 28, 29, 30, 36, 37, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 31, 2, 4, 5, 32, 7, 8, 33, 13, 13, 13, 14, 14, 16, 34, 18, 20, 22, 22, 36, 36, 37, 37, 39, 38, 67, 66, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 24, 25, 18, 19, 20, 21, 16, 2, 36, 2, 25, 7, 6, 37, 16, 22, 22, 24, 6, 7, 14, 15, 6, 16, 36, 25, 4, 7, 6, 24, 6, 19, 8, 10, 11, 13, 12, 13, 22, 14, 18, 16, 16, 24, 20, 21, 19, 36, 36, 36, 7, 6, 7, 6, 2, 4, 6, 10, 25, 18, 16, 12, 24, 7, 3, 5, 12, 25, 20, 16, 22, 16, 27, 18, 36, 37, 3, 4, 6, 8, 10, 13, 14, 16, 24, 18, 20, 17, 22, 2, 7, 12, 36, 11, 25, 5, 15, 21, 37, 27, 9, 36, 7, 2, 10, 20, 12, 16, 8, 6, 11, 22, 18, 3, 68, 36, 14, 5, 4, 37, 8, 2, 25, 9, 11, 21, 69, 15, 13, 7, 12, 24, 14, 34, 6, 28, 21, 12, 25, 16, 7, 7, 17, 19, 36, 27, 7, 70, 10, 24, 37, 37, 5, 10, 2, 3, 6, 7, 18, 14, 16, 20, 22, 25, 17, 37, 27, 15, 16, 6, 7, 37, 36, 16, 37, 25, 12, 27, 2, 37, 6, 36, 16, 7, 36, 23, 35
        };

        private static readonly int StageCount = 71;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="externalId"></param>
        /// <returns></returns>
        public static int ToInternalID(int externalId)
        {
            if (externalId < 0)
                return 0;

            if (externalId >= ExternalToInternal.Length)
            {
                return (externalId - ExternalToInternal.Length) + StageCount;
            }

            return ExternalToInternal[externalId];
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="internalID"></param>
        /// <returns></returns>
        public static int ToExternalID(int internalID)
        {
            for (int i = 0; i < ExternalToInternal.Length; i++)
            {
                if (ExternalToInternal[i] == internalID)
                    return i;
            }
            var stupid = ExternalToInternal.Length;
            return (internalID - StageCount) + ExternalToInternal.Length;
        }
    }
}
