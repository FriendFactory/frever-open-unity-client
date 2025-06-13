using UnityEngine;

namespace Common.Abstract
{
    public abstract class BaseContextPanel<TModel>: MonoBehaviour, IContextInitializable<TModel>
    {
        public TModel ContextData { get; private set; }
        public bool IsInitialized { get; private set; }
        public bool IsDestroyed { get; private set; }
        
        protected virtual bool IsReinitializable => false;

        private void OnDestroy()
        {
            IsDestroyed = true;
            
            if (!IsInitialized) return;
                
            CleanUp();
        }

        public void Initialize(TModel model)
        {
            if (IsDestroyed)
            {
                Debug.LogWarning($"[{GetType().Name}] Attempt to initialize already destroyed GO");
                return;
            }
            
            if (IsReinitializable)
            {
                if (ContextData != null)
                {
                    CleanUp();
                }
            }
            else
            {
                if (IsInitialized)
                {
                    Debug.LogError($"[{GetType().Name}] Panel is already initialized. Clean-up it before the re-initialization.", gameObject);
                    return;
                }
            }
            
            ContextData = model;
            
            OnInitialized();
            IsInitialized = true;
        }

        public void CleanUp()
        {
            if (!IsReinitializable && !IsInitialized)
            {
                return;
            }
            
            BeforeCleanUp();
            IsInitialized = false;
            ContextData = default;
        }
        
        protected abstract void OnInitialized();
        
        protected virtual void BeforeCleanUp() { }
    }
}