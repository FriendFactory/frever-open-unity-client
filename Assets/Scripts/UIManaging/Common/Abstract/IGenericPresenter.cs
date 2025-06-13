namespace UIManaging.Common.Abstract
{
    public interface IGenericPresenter<TModel, TView>
    {
        bool IsInitialized { get; }

        void Initialize(TModel model, TView view);
        void CleanUp();
    }
}