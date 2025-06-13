using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.PopupSystem.Popups.Views
{
    internal abstract class PrivacyToggleBase<T> : MonoBehaviour where T : Enum
    {
        [SerializeField] protected T targetAccess;
        [SerializeField] protected TextMeshProUGUI targetText;
        [Header("Optional")] [SerializeField] protected Toggle targetToggle;

        public event Action<T> OnValueSelected;

        public T TargetAccess => targetAccess;

        public abstract void SetValue(T access);
        protected abstract string ToText();

        public void Initialize()
        {
            targetText.text = ToText();
        }
        
        private void Awake()
        {
            if (targetToggle == null)
            {
                targetToggle = GetComponent<Toggle>();
            }
        }

        private void OnEnable()
        {
            targetToggle.onValueChanged.AddListener(OnValueChanged);
        }

        private void OnDisable()
        {
            targetToggle.onValueChanged.RemoveListener(OnValueChanged);
        }

        private void OnValueChanged(bool value)
        {
            if (value)
            {
                OnValueSelected?.Invoke(targetAccess);
            }
        }
    }
}