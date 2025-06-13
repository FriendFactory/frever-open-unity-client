using System;
using Abstract;
using UnityEngine;
using UnityEngine.UI;

namespace Common.Abstract
{
    public abstract class BaseContextToggle<T>: BaseContextDataView<T>
    {
        [SerializeField] protected Toggle _toggle;

        public event Action<bool> ValueChanged;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_toggle) return;

            _toggle = GetComponent<Toggle>();
        }
#endif

        private void OnEnable()
        {
            _toggle.onValueChanged.AddListener(OnValueChanged);
        }
        
        private void OnDisable()
        {
            _toggle.onValueChanged.RemoveListener(OnValueChanged);
        }

        protected virtual void OnValueChanged(bool isOn)
        {
            ValueChanged?.Invoke(isOn);
        }
    }
}