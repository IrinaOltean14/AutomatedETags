namespace AutomatedETags
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class EnableETagAttribute : Attribute
    {
        public bool IsWeak { get; }

        public EnableETagAttribute(bool isWeak = false)
        {
            IsWeak = isWeak;
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class SkipETagAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class WeakETagAttribute : EnableETagAttribute
    {
        public WeakETagAttribute() : base(isWeak: true)
        {
        }
    }
}
