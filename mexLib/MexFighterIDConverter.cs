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
            return 
                (   
                    internalId >= BaseCharacterCount - InternalSpecialCharCount && 
                    internalId < characterCount - InternalSpecialCharCount
               );
        }

        /// <summary>
        /// Converts an internal character ID (CKIND) to its external (FTKIND) representation.
        /// Handles added characters and special character remapping.
        /// </summary>
        public static int ToExternalID(int internalID, int characterCount)
        {
            // Special hardcoded case (Popo)
            if (internalID == 11)
                return characterCount - 1;

            int addedChars = characterCount - BaseCharacterCount;

            int specialStart = characterCount - InternalSpecialCharCount;
            bool isSpecial = internalID >= specialStart;

            int addedRangeStart = specialStart - addedChars;

            // Case 1: internal ID falls into the shifted "added characters" range
            if (internalID >= addedRangeStart && !isSpecial)
            {
                int baseOffset = BaseCharacterCount - ExternalSpecialCharCount;
                int internalOffset = internalID - (BaseCharacterCount - InternalSpecialCharCount);
                return baseOffset + internalOffset;
            }

            // Start with a base external ID
            int externalID = internalID;

            // collapse space for added characters if special
            if (isSpecial)
                externalID -= addedChars;

            // apply lookup remapping if within bounds
            if (externalID < InternalToExternal.Length)
                externalID = InternalToExternal[externalID];

            // restore offset for special characters
            if (isSpecial)
                externalID += addedChars;

            return externalID;
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
