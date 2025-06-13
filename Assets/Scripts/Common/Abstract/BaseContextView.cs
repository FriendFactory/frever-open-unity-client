using Abstract;
using UnityEngine;

namespace Common.Abstract
{
    public abstract class BaseContextView<TModel>: MonoBehaviour, IContextInitializable<TModel>, IDisplayable
    {
        public TModel ContextData { get; private set; }
        public bool IsInitialized { get; private set; }
        public bool IsDisplayed { get; private set; }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        protected virtual void Awake() { }

        protected virtual void OnDestroy()
        {
            if (!IsInitialized)  return;

            CleanUp();
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Initialize(TModel model)
        {
            if (IsInitialized)
            {
                Debug.LogError($"[{GetType().Name}] Object is already initialized. Clean-up it before the re-initialization.", gameObject);
                return;
            }

            ContextData = model;
            
            OnActivated();
            OnInitialized();
            IsInitialized = true;
        }

        public void CleanUp()
        {
            if (!IsInitialized)
            {
                Debug.LogError($"[{GetType().Name}] Object is not initialized. Object should be initialized before clean-up.", gameObject);
                return;
            }
            
            OnDeactivated();
            BeforeCleanUp();
            
            IsInitialized = false;
            ContextData = default;
        }

        public void Show()
        {
            if (!IsInitialized) return;
            
            if (IsDisplayed) return;
            
            gameObject.SetActive(true);
            
            OnShow();
            IsDisplayed = true;
        }

        public void Hide()
        {
            if (!IsInitialized) return;
            if (!IsDisplayed) return;
            
            gameObject.SetActive(false);
            
            OnHide();
            IsDisplayed = false;
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected abstract void OnInitialized();
        
        protected virtual void BeforeCleanUp() { }
        protected virtual void OnShow() { }
        protected virtual void OnHide() { }
        protected virtual void OnActivated() {}
        protected virtual void OnDeactivated() {}
    }
}