using System;
using Abstract;
using UnityEngine;

namespace UIManaging.Pages.Common.SongOption
{
    public abstract class BaseContextlessView: MonoBehaviour, IInitializable, IDisplayable
    {
        public bool IsInitialized { get; private set; }
        public bool IsDisplayed { get; private set; }
        
        protected virtual void Awake() { }

        protected virtual void OnDestroy()
        {
            if (!IsInitialized) return;
            
            CleanUp();
        }

        public void Initialize()
        {
            if (IsInitialized)
            {
                Debug.LogError($"[{GetType().Name}] Object is already initialized. Clean-up it before the re-initialization.", gameObject);
                return;
            }
            
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
        }

        public virtual void Show()
        {
            if (!IsInitialized) return;
            
            gameObject.SetActive(true);
            
            OnShow();
            IsDisplayed = true;
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
            
            if (!IsInitialized) return;
            
            OnHide();
            IsDisplayed = false;
        }

        protected abstract void OnInitialized();
        
        protected virtual void BeforeCleanUp() { }
        protected virtual void OnShow() { }
        protected virtual void OnHide() { }
        protected virtual void OnActivated() {}
        protected virtual void OnDeactivated() {}
    }
}