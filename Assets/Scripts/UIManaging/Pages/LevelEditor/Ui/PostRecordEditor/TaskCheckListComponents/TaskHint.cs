using DG.Tweening;
using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.TaskCheckListComponents
{
    internal class TaskHint : MonoBehaviour
    {
        private const float FADE_OUT_DURATION = 0.5f;

        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private CanvasGroup _canvasGroup;
        
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        protected Sequence AnimationSequence { get; private set; }
        protected RectTransform RectTransform => _rectTransform;
        protected Vector3 StartPosition { get; private set; }
        
        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            StartPosition = _rectTransform.position;
        }

        private void OnDisable()
        {
            _canvasGroup.alpha = 0f;
            AnimationSequence?.Kill();
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void Show()
        {
            gameObject.SetActive(true);
            _canvasGroup.alpha = 1f;
            PlayAnimation();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected virtual void PlayAnimation()
        {
            PrepareAnimation();
        }

        protected void FadeOut()
        {
            _canvasGroup.DOFade(0, FADE_OUT_DURATION).OnComplete(Hide);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void ReturnToStartPosition()
        {
            if (StartPosition != Vector3.zero)
            {
                _rectTransform.position = StartPosition;
            }
        }
        
        private void PrepareAnimation()
        {
            ReturnToStartPosition();
            AnimationSequence?.Kill();
            AnimationSequence = DOTween.Sequence();
        }
    }
}
