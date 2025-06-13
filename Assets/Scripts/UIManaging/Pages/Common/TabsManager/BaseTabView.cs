using System;
using Abstract;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.Common.TabsManager
{
    public class BaseTabView : BaseContextDataView<TabModel>, ITabView
    {
        [SerializeField] private TabUpdateMarker _feedTabUpdateMarker;
        [SerializeField] private Toggle _toggle;

        private RectTransform _rectTransform;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public int Index => ContextData.Index;
        
        public Toggle Toggle => _toggle;

        public RectTransform RectTransform
        {
            get
            {
                if (_rectTransform == null)
                {
                    _rectTransform = GetComponent<RectTransform>();
                }

                return _rectTransform;
            }
        }

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action<int, bool> OnToggleValueChangedEvent;
        public event Action<int, string> OnSelected;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected virtual void OnEnable()
        {
            _toggle.onValueChanged.AddListener(OnToggleValueChanged);
        }

        private void OnDisable()
        {
            _toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void SetValueSilent(bool value)
        {
            _toggle.SetIsOnWithoutNotify(value);

            if (ContextData.ShowUpdateMarker)
            {
                _feedTabUpdateMarker.SetIsOpened(value);
            }
            
            OnBeforeOnToggleValueChangedEvent(value);
        }

        public virtual void RefreshVisuals()
        {
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnInitialized()
        {
            if (_feedTabUpdateMarker != null)
            {
                _feedTabUpdateMarker.gameObject.SetActive(ContextData.ShowUpdateMarker);
            }
        }

        protected virtual void OnBeforeOnToggleValueChangedEvent(bool value) { }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnToggleValueChanged(bool value)
        {
            if (ContextData.ShowUpdateMarker)
            {
                _feedTabUpdateMarker.SetIsOpened(value);
            }
            
            OnBeforeOnToggleValueChangedEvent(value);
            InvokeOnToggleValueChangedEvent(value);
            if(value) OnSelected?.Invoke(ContextData.Index, ContextData.Name);
        }

        private void InvokeOnToggleValueChangedEvent(bool value)
        {
            OnToggleValueChangedEvent?.Invoke(ContextData.Index, value);
        }
    }
}