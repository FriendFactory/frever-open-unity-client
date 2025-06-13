namespace Abstract
{
    public interface IInitializable
    {
        bool IsInitialized { get; }

        void Initialize();
        void CleanUp();
    }
}