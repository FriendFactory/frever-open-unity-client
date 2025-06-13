namespace UIManaging.Common.Abstract
{
    public abstract class GenericPresenter<TModel, TView>: IGenericPresenter<TModel, TView>
    {
        protected TModel Model { get; private set; }
        protected TView View { get; private set; }

        public bool IsInitialized { get; private set; }
        
        public void Initialize(TModel model, TView view)
        {
            Model = model;
            View = view;
            
            OnInitialized();

            IsInitialized = true;
        }

        protected abstract void OnInitialized();
        protected abstract void BeforeCleanUp();

        public void CleanUp()
        {
            BeforeCleanUp();

            IsInitialized = false;
        }
    }
}