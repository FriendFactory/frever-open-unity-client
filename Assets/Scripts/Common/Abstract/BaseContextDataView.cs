using Common.Abstract;
using Common.UI;

namespace Abstract
{
    public abstract class BaseContextDataView<T> : BaseInteractiveUI, IContextInitializable<T>
    {
        public T ContextData { get; private set; }
        public bool IsDestroyed { get; private set; }
        public bool IsInitialized { get; private set; }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Initialize(T model)
        {
            if (ContextData != null)
            {
                CleanUp();
            }
        
            ContextData = model;
            IsInitialized = true;
            OnInitialized();
        }

        public void CleanUp()
        {
            BeforeCleanup();
            ContextData = default(T);
            IsInitialized = false;
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected abstract void OnInitialized();

        protected virtual void BeforeCleanup() {}

        protected virtual void OnDestroy()
        {
            IsDestroyed = true;
            CleanUp();
        }
    }
}