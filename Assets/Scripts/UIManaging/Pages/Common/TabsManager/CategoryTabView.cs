using Common;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.Common.TabsManager
{
    public class CategoryTabView : TabView
    {
        [SerializeField] private float _transitionDuration = 0.1f;

        [SerializeField] private ContentSizeFitter _contentSizeFitter;
        [SerializeField] private TMP_FontAsset _selectedFont;
        [SerializeField] private TMP_FontAsset _unSelectedFont;
        [SerializeField] private GameObject _newIcon;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            _contentSizeFitter.enabled = false;
            CoroutineSource.Instance.ExecuteWithFrameDelay(EnableContentSizeFitter);
            SetIsNew();
        }

        private void EnableContentSizeFitter()
        {
            _contentSizeFitter.enabled = true;
        }

        protected override void OnBeforeOnToggleValueChangedEvent(bool value)
        {
            base.OnBeforeOnToggleValueChangedEvent(value);

            if (value)
            {
                DoTransitionToSelectedState();
            }
            else
            {
                DoTransitionToUnselectedState();
            }
        }
    
        private void DoTransitionToSelectedState()
        {
            DoFadeTransition(true);
        }

        private void DoTransitionToUnselectedState()
        {
            DoFadeTransition(false);
        }

        private void DoFadeTransition(bool selected)
        {
            _tabNameText.DOKill();
            var font = selected ? _selectedFont : _unSelectedFont;
            _tabNameText.font = font;
            var alpha = selected ? 1f : 0.7f;
            _tabNameText.DOFade(alpha, _transitionDuration).SetUpdate(true);
        }

        public override void RefreshVisuals()
        {
            base.RefreshVisuals();
            DoFadeTransition(Toggle.isOn);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _tabNameText.DOKill();
        }
        
        protected virtual void SetIsNew()
        {
            if (!_newIcon) return;
            _newIcon.SetActive(ContextData.ContainsNew);
        }
    }
}