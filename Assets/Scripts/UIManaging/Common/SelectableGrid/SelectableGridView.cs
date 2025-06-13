using System;
using Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Common.SelectableGrid
{
    public class SelectableGridView : MonoBehaviour
    {
        public event Action<long, bool> OnSelectedStatusChangedEvent;
        public event Action<SelectableGridView> OnIdChangedEvent;

        [SerializeField] private Image _selectableBackgroundImage;
        [SerializeField] private Toggle _toggle;
        [SerializeField] private GameObject _selectableGameObject;

        public long Id { get; private set; }
        private ISelectableGridViewProvider _iSelectableGridViewProvider;
        private Color _initialColor;

        private void Awake()
        {
            _iSelectableGridViewProvider = GetComponentInParent<ISelectableGridViewProvider>();
            _initialColor = _selectableBackgroundImage.color;
            
            if (_iSelectableGridViewProvider != null)
            {
                _iSelectableGridViewProvider.OnIdChangedEvent += OnIdChanged;
            }
            else
            {
                Debug.LogError($"Component with {nameof(ISelectableGridViewProvider)} interface was not found in parent of {gameObject.name} gameObject", gameObject);
            }
            
            SetSelectedModeActive(false);
        }

        private void OnDestroy()
        {
            if (_iSelectableGridViewProvider != null)
            {
                _iSelectableGridViewProvider.OnIdChangedEvent -= OnIdChanged;
            }
        }

        private void OnIdChanged()
        {
            Id = _iSelectableGridViewProvider.Id;
            OnIdChangedEvent?.Invoke(this);
        }

        public void SetToggle(bool value)
        {
            _toggle.isOn = value;
            OnToggleValueChanged(value);
        }
        
        private void OnEnable()
        {
            _toggle.onValueChanged.AddListener(OnToggleValueChanged);
        }

        private void OnDisable()
        {
            _toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
        }

        private void OnToggleValueChanged(bool value)
        {
            var alpha = value ? 0f : 0.5f;
            _selectableBackgroundImage.color = _initialColor.SetAlpha(alpha);
            OnSelectedStatusChangedEvent?.Invoke(Id, value);
        }

        public void SetSelectedModeActive(bool value)
        {
            _selectableGameObject.SetActive(value);
            _toggle.gameObject.SetActive(value);
        }
    }
}