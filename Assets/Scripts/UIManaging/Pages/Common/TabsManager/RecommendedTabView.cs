using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.Common.TabsManager
{
    public class RecommendedTabView : BaseTabView
    {
        [SerializeField] private float _transitionDuration = 0.1f;
        [SerializeField] private Image _recommendedIcon;
        [SerializeField] private GameObject _newIcon;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            SetIsNew();
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
            _recommendedIcon.DOKill();
            var alpha = selected ? 1f : 0.7f;
            _recommendedIcon.DOFade(alpha, _transitionDuration).SetUpdate(true);
        }

        public override void RefreshVisuals()
        {
            base.RefreshVisuals();
            DoFadeTransition(Toggle.isOn);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _recommendedIcon.DOKill();
        }
        
        protected virtual void SetIsNew()
        {
            if (_newIcon == null)
            {
                return;
            }
            
            _newIcon.SetActive(ContextData.ContainsNew);
        }
    }
}