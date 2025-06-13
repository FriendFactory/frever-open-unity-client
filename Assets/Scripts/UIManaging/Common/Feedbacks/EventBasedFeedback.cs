using UnityEngine;

namespace UIManaging.Common.Buttons
{
    public abstract class EventBasedFeedback<T>: MonoBehaviour where T: Component
    {
        private T _source;

        protected T Source => _source; 
        
    #if UNITY_EDITOR
        private void OnValidate()
        {
            if (_source) return;

            _source = GetComponent<T>();
        }
    #endif

        protected virtual void Awake()
        {
            if (!_source) _source = GetComponent<T>();
        }
        
        private void OnEnable()
        {
            Subscribe();
        }
        
        private void OnDisable()
        {
            Unsubscribe();
        }

        protected abstract void Subscribe();
        protected abstract void Unsubscribe();
        protected abstract void PlayFeedback();
    }
}