namespace UIManaging.PopupSystem.Configurations
{
    public class GenericViewPopupConfiguration : PopupConfiguration
    {
        public Alignment Alignment { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public enum Alignment
    {
        Left,
        Top,
        Right,
        Bottom,
        Center
    }
}