using System.Collections;
using DG.Tweening;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;

namespace UIManaging.PopupSystem.Popups
{
    internal sealed class SlowConnectionPopup : InformationPopup<InformationPopupConfiguration>
    {
        private const float AUTO_HIDE_TIME = 5f;
        
        private Vector2 _slideInPosition;
        private Vector2 _startPosition;

        [SerializeField] private RectTransform _bodyRect;

        private void Awake()
        {
            Setup();
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public override void Show()
        {
            base.Show();
            SlideIn();
            StartCoroutine(AutoHideCounter());
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnConfigure(InformationPopupConfiguration configuration)
        {

        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void Setup()
        {
            var anchoredPosition = _bodyRect.anchoredPosition;
            _slideInPosition = new Vector3(anchoredPosition.x, anchoredPosition.y - _bodyRect.rect.height);
            _startPosition = anchoredPosition;
        }

        private IEnumerator AutoHideCounter()
        {
            yield return new WaitForSeconds(AUTO_HIDE_TIME);
            SlideOut();
        }
        
        private void SlideOut()
        {
            _bodyRect.DOAnchorPos(_startPosition, 0.4f).OnComplete(Hide);
        }
        
        private void SlideIn()
        {
            _bodyRect.DOAnchorPos(_slideInPosition, 1f);
        }
    }
}