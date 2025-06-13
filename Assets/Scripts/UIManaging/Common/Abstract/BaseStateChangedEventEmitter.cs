using UnityEngine;
using UnityEngine.Events;

namespace UIManaging.Common.Abstract
{
    public abstract class BaseStateChangedEventEmitter<TTarget>: MonoBehaviour where TTarget: Component
    {
        [SerializeField] private TTarget _target;
        
        public UnityEvent<bool> StateChanged = new ();
        
        public TTarget Target => _target ? _target : _target = GetComponent<TTarget>();

        #if UNITY_EDITOR
        private void OnValidate()
        {
            _target = GetComponent<TTarget>();
        }
        #endif

        protected virtual void OnEnable()
        {
            Subscribe();
        }

        protected virtual void OnDisable()
        {
            Unsubscribe();
        }

        protected abstract void Subscribe();
        protected abstract void Unsubscribe();

        protected virtual void OnStateChanged(bool isOn)
        {
            StateChanged.Invoke(isOn);
        }
    }
}