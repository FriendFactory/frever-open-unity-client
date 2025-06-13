using System;
using Common.Abstract;
using EnhancedUI.EnhancedScroller;
using Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Common.Dropdown
{
    [RequireComponent(typeof(Button))]
    public sealed class Dropdown : BaseContextView<DropdownModel>, IEnhancedScrollerDelegate
    {
        [SerializeField] private Button _button;
        [SerializeField] private Button _outsideClickCatcher;
        
        [Space]
        [SerializeField] private string _placeholder;
        [SerializeField] private TMP_Text _label;
        [SerializeField] private Transform _arrow;

        [Space] 
        [SerializeField] private Image _background;
        [SerializeField] private Sprite _normalBackground;
        [SerializeField] private Sprite _selectedBackground;

        [Space] 
        [SerializeField] private EnhancedScroller _scroller;
        [SerializeField] private EnhancedScrollerCellView _listElement;
        [SerializeField] private float _elementHeight = 135.0f;
        
        public int SelectedOption { get; private set; } = -1;

        public event Action<int> OnOptionSelected; 

        private void Reset()
        {
            _button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            _scroller.Delegate = this;
            _outsideClickCatcher.onClick.AddListener(Toggle);
            _button.onClick.AddListener(Toggle);
            ApplyFoldedStyle();
        }

        private void OnDisable()
        {
            _scroller.Delegate = null;
            _button.onClick.RemoveAllListeners();
            _outsideClickCatcher.onClick.RemoveAllListeners();
            _scroller.ClearActive();

            if (IsInitialized)
            {
                CleanUp();
            }
        }

        protected override void OnInitialized()
        {
            _label.text = _placeholder;
            SelectedOption = -1;
            
            _scroller.ReloadData();
        }

        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            return ContextData?.Options.Count ?? 0;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return _elementHeight;
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var cell = _scroller.GetCellView(_listElement);

            var model = new DropdownElementModel(dataIndex, ContextData.Options[dataIndex]);
            var view = cell.GetComponent<DropdownElement>();
            view.OnSelected += OnElementClicked;
            view.Initialize(model);

            return cell;
        }

        private void OnElementClicked(int index)
        {
            _label.text = ContextData.Options[index];
            SelectedOption = index;
            OnOptionSelected?.Invoke(index);
            ApplyFoldedStyle();
        }
        
        private void Toggle()
        {
            if (_scroller.gameObject.activeInHierarchy)
            {
                ApplyFoldedStyle();
                return;
            }
            
            ApplyExpendedStyle();
        }

        private void ApplyFoldedStyle()
        {
            _background.sprite = _normalBackground;
            _scroller.SetActive(false);
            ToggleArrowIcon(false);
            _outsideClickCatcher.interactable = false;
        }

        private void ApplyExpendedStyle()
        {
            _background.sprite = _selectedBackground;
            _scroller.SetActive(true);
            ToggleArrowIcon(true);
            _outsideClickCatcher.interactable = true;
        }

        private void ToggleArrowIcon(bool value)
        {
            var direction = value ? Vector3.forward : Vector3.back;
            _arrow.transform.eulerAngles = direction * 90.0f;
        }
    }
}