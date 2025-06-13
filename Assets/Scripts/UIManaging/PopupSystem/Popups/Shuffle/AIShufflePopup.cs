using System;
using System.Linq;
using AdvancedInputFieldPlugin;
using Modules.LevelManaging.Editing.LevelManagement;
using UIManaging.Animated.Behaviours;
using UIManaging.Common.InputFields;
using UnityEngine;

namespace UIManaging.PopupSystem.Popups.Shuffle
{
    public class AIShufflePopup : AssetTypeSelectionPopup
    {
        [SerializeField] private IgnoredDeselectableAreaAdvancedInputField _promptInputField;
        [SerializeField] private SlideInOutBehaviour _inputAnimator;
        [SerializeField] private RectTransform _ignoreDeselectRect;

        private static int _cachedKeyboardHeight; 
        private static int _keyboardHeightOffset;
        private bool _isAnimatorInitialized;

        private void Awake()
        {
            NativeKeyboardManager.AddKeyboardHeightChangedListener(KeyboardHeightChanged);
        }

        protected override void OnConfirm()
        {
            var shuffleModel = new AIShuffleModel(
                AssetTypeListModel.SelectedItems.Select(model => model.Type).Aggregate((type1, type2) => type1 | type2),
                _promptInputField.Text);
            Config.ConfirmAction?.Invoke(shuffleModel);
            Hide();
        }

        public override void Show()
        {
            base.Show();
            _promptInputField.AddIgnoreDeselectOnRect(_ignoreDeselectRect);
        }

        protected override void OnHidden()
        {
            base.OnHidden();
            _inputAnimator.Hide();
            _promptInputField.ShouldBlockDeselect = false;
            _promptInputField.ManualDeselect();
            _promptInputField.RemoveIgnoreDeselectOnRect(_ignoreDeselectRect);
        }

        private void OnInputSelectionChanged(bool isSelected)
        {
            if (isSelected) _inputAnimator.SlideIn();
        }
        
        private void KeyboardHeightChanged(int height)
        {
            if (height == 0) return;

            var scaleFactor = GetCanvasHeightScaleFactor();
            var actualKeyboardHeight = (int) (height * scaleFactor) + _keyboardHeightOffset;

            if (_isAnimatorInitialized && _cachedKeyboardHeight == actualKeyboardHeight) return;

            _cachedKeyboardHeight = actualKeyboardHeight;
            _inputAnimator.InitSequence(new Vector3(0, _cachedKeyboardHeight), Vector3.zero);
            _isAnimatorInitialized = true;
            _inputAnimator.SlideIn();
            
            NativeKeyboardManager.RemoveKeyboardHeightChangedListener(KeyboardHeightChanged);
            _promptInputField.OnSelectionChanged.AddListener(OnInputSelectionChanged);
        }
        
        private float GetCanvasHeightScaleFactor()
        {
            return transform.root.GetComponent<RectTransform>().rect.height / Screen.height;
        }
    }
    
}