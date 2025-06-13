namespace Common.Abstract
{
    public interface IContextInitializable<TModel>
    {
        TModel ContextData { get; }
        bool IsInitialized { get; }

        void Initialize(TModel model);
        void CleanUp();
    }
}