using System;
using Common;
using UI.UIAnimators;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.PopupSystem.Popups
{
    internal sealed class InformationMessage : InformationPopup<InformationMessageConfiguration>
    {
        [SerializeField] private float waitBeforeHide = 3f;
        [SerializeField] private RectTransform _bodyRectTransform;
        [SerializeField] private Button _button;
        [SerializeField] private HorizontalOrVerticalLayoutGroup _layoutGroup;
        [SerializeField] private BaseUiAnimationPlayer _animationPlayer;

        private Action _onClick;

        protected override void OnConfigure(InformationMessageConfiguration configuration)
        {
            base.OnConfigure(configuration);
            
            var bodyParent = _bodyRectTransform.parent.GetComponent<RectTransform>();
            _bodyRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, bodyParent.rect.width * 0.9f);
            _animationPlayer.PlayShowAnimation(OnShowAnimationPlayed);

            _onClick = configuration.OnClickAction;
            var hasClickAction = _onClick != null;
            _button.enabled = hasClickAction;

            if (hasClickAction)
            {
                _button.onClick.AddListener(OnButtonClicked);
            }

            RefreshLayoutGroup();
        }

        private void OnButtonClicked()
        {
            _onClick?.Invoke();
            _button.enabled = false;
        }

        private void RefreshLayoutGroup()
        {
            if (_layoutGroup != null)
            {
                _layoutGroup.enabled = false;
                CoroutineSource.Instance.ExecuteWithFrameDelay(() => _layoutGroup.enabled = true);   
            }
        }

        private void OnShowAnimationPlayed()
        {
            CoroutineSource.Instance.ExecuteWithRealtimeDelay(waitBeforeHide,
                () => _animationPlayer.PlayHideAnimation(Hide));
        }
    }
}