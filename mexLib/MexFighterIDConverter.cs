using HSDRaw.MEX;

namespace mexLib
{
    public class MexFighterIDConverter
    {
        private static int BaseCharacterCount { get; } = 0x21;

        private static int InternalSpecialCharCount { get; } = 6;

        private static int ExternalSpecialCharCount { get; } = 7;

        //private readonly static int[] ExternalToInternal = {
        //    0x02, 0x03, 0x01, 0x18, 0x04, 0x05, 0x06,
        //    0x11, 0x00, 0x12, 0x10, 0x08, 0x09, 0x0C,
        //    0x0A, 0x0F, 0x0D, 0x0E, 0x13, 0x07, 0x16,
        //    0x14, 0x15, 0x1A, 0x17, 0x19, 0x1B, 0x1D,
        //    0x1E, 0x1F, 0x1C, 0x20, 0x0A
        //};

        private readonly static int[] InternalToExternal = {
            0x08, 0x02, 0x00, 0x01, 0x04, 0x05, 0x06,
            0x13, 0x0B, 0x0C, 0x0E, 0x20, 0x0D, 0x10,
            0x11, 0x0F, 0x0A, 0x07, 0x09, 0x12, 0x15,
            0x16, 0x14, 0x18, 0x03, 0x19, 0x17, 0x1A,
            0x1E, 0x1B, 0x1C, 0x1D, 0x1F};

        /// <summary>
        /// 
        /// </summary>
        /// <param name="internalId"></param>
        /// <returns></returns>
        public static bool IsMexFighter(int internalId, int characterCount)
        {
            return (internalId >= 0x21 - 6 && internalId < characterCount - 6);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="internalID"></param>
        /// <param name="characterCount"></param>
        /// <returns></returns>
        public static int ToExternalID(int internalID, int characterCount)
        {
            int addedChars = characterCount - BaseCharacterCount;
            bool isSpecialCharacter = internalID >= characterCount - InternalSpecialCharCount;

            if (internalID >= characterCount - InternalSpecialCharCount - addedChars &&
                !isSpecialCharacter)
                return (BaseCharacterCount - ExternalSpecialCharCount) + (internalID - (BaseCharacterCount - InternalSpecialCharCount));

            int externalId = internalID + (isSpecialCharacter ? -addedChars : 0);

            if (externalId < InternalToExternal.Length)
                externalId = InternalToExternal[externalId];

            if (isSpecialCharacter)
                externalId += addedChars;

            if (internalID == 11) // POPO special case
                externalId = characterCount - 1;

            return externalId;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="externalId"></param>
        /// <param name="characterCount"></param>
        /// <returns></returns>
        public static int ToInternalID(int externalId, int characterCount)
        {
            for (int i = 0; i < characterCount; i++)
                if (ToExternalID(i, characterCount) == externalId)
                    return i;

            return -1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mexData"></param>
        /// <param name="internalID"></param>
        /// <returns></returns>
        public static bool IsSpecialCharacterInternal(MEX_Data mexData, int internalID)
        {
            return internalID >= mexData.MetaData.NumOfInternalIDs - InternalSpecialCharCount;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mexData"></param>
        /// <param name="externalID"></param>
        /// <returns></returns>
        public static bool IsSpecialCharacterExternal(MEX_Data mexData, int externalID)
        {
            return externalID >= mexData.MetaData.NumOfExternalIDs - ExternalSpecialCharCount;
        }
    }
}
