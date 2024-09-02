namespace mexLib.Attributes
{
    public enum MexLinkType
    {
        Fighter,
        Stage,
        Music,
        Sound,
        Series
    }

    public class MexLinkAttribute : Attribute
    {
        public MexLinkType Link { get; internal set; }

        public MexLinkAttribute(MexLinkType link)
        {
            Link = link;
        }
    }
}
