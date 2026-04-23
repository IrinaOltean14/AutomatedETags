namespace AutomatedETags
{
    public enum ETagMode
    {
        Global,
        OptIn
    }

    public enum ETagAlgorithm
    {
        SHA256,
        MD5,
        XxHash64
    }

    public class ETagOptions
    {
        public ETagMode Mode { get; set; } = ETagMode.Global;

        public ETagAlgorithm Algorithm { get; set; } = ETagAlgorithm.SHA256;

        public long MaxBodySize { get; set; } = 64 * 1024; // 64 KB

        public bool UseWeakETags { get; set; } = false;
    }
}
