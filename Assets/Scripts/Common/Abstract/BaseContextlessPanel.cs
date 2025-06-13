using Abstract;
using UnityEngine;

namespace Common.Abstract
{
    public abstract class BaseContextlessPanel: MonoBehaviour, IInitializable
    {
        public bool IsInitialized { get; private set; }
        public bool IsDestroyed { get; private set; }
            
        protected virtual bool IsReinitializable => false;
        
        protected virtual void OnDestroy()
        {
            IsDestroyed = true;
            
            if (!IsInitialized) return;
                
            CleanUp();
        }

        public void Initialize()
        {
            if (IsDestroyed)
            {
                Debug.LogWarning($"[{GetType().Name}] Attempt to initialize already destroyed GO");
                return;
            }

            if (IsReinitializable)
            {
                CleanUp();
            }
            
            if (IsInitialized)
            {
                Debug.LogError($"[{GetType().Name}] Object is already initialized. Clean-up it before the re-initialization.", gameObject);
                return;
            }
            
            OnInitialized();
            IsInitialized = true;
        }

        public void CleanUp()
        {
            if (!IsReinitializable && !IsInitialized)
            {
                Debug.LogError($"[{GetType().Name}] Object is not initialized. Object should be initialized before clean-up.", gameObject);
                return;
            }
            
            BeforeCleanUp();
            IsInitialized = false;
        }
        
        protected abstract void OnInitialized();
        
        protected virtual void BeforeCleanUp() { }
    }
}