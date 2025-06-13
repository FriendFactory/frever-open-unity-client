using System;
using Bridge.Models.ClientServer.Level.Full;
using UIManaging.Animated;
using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.CaptionsPanel
{
    internal sealed class TextSettingsPanel : MonoBehaviour
    {
        [SerializeField] private KeyboardAnchoredUiAnimator _animator;
        [SerializeField] private TextAlignmentButton _alignmentButton;

        public event Action<CaptionTextAlignment> AlignmentSelected;

        private void Awake()
        {
            _alignmentButton.Clicked += x =>
            {
                AlignmentSelected?.Invoke(x);
            };
        }

        public void Show(CaptionTextAlignment alignment)
        {
            gameObject.SetActive(true);
            _animator.SlideUpInputField();
            _alignmentButton.Setup(alignment);
        }

        public void Hide()
        { 
            _animator.SlideDownInputField();
            gameObject.SetActive(false);
        }
    }
}
