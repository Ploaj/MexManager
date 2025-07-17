namespace mexLib.Types
{
    /// <summary>
    /// 
    /// </summary>
    public class MexCodeCompileError
    {
        public int LineIndex { get; set; }

        public string Description { get; set; }

        public MexCodeCompileError(int lineIndex, string description)
        {
            LineIndex = lineIndex;
            Description = description;
        }

        public override string ToString()
        {
            if (LineIndex != -1)
                return $"Error: {Description} on line {LineIndex}";
            else
                return $"Error: {Description}";
        }
    }

}
