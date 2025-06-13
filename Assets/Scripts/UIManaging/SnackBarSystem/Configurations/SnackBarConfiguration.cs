namespace UIManaging.SnackBarSystem.Configurations
{
    public abstract class SnackBarConfiguration
    {
        internal abstract SnackBarType Type { get; }

        public float? Time;
        public string Title;
    }
}