using System;
using System.Collections;
using System.Threading.Tasks;
using TipsManagment.Args;
using UIManaging.Pages.Feed.Core;
using UnityEngine;
using UnityEngine.UI;

namespace TipsManagment
{
    internal class SimplifiedSwipeTip : OnboardingTip
    {
        private const float FADE_TIME = .5F;
        private const float FADE_DELAY = .5F;
        
        private FeedScrollView _feedScroll;

        [SerializeField] private Button _closeButton; 
        [SerializeField] private CanvasGroup _canvasGroup;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        protected override void Update()
        {
#if UNITY_EDITOR
            var pos = Input.mousePosition;
            if (!Input.GetMouseButton(0)) return;
#else
            if (Input.touchCount == 0) return;
            var pos = Input.GetTouch(0).position;
#endif

            Close();
        }
     
        public override void Init(TipArgs args)
        {
            base.Init(args);
            _closeButton.onClick.RemoveAllListeners();
            _closeButton.onClick.AddListener(OnCloseClick);
        }

        public override void Show()
        {
            _canvasGroup.alpha = 0f;
            base.Show();
            
            StartCoroutine(FadeRoutine(true));
        }

        public override async Task Activate()
        {
            await base.Activate();
            if (TargetTransform is null) return; // Check if target destoroyed (time sensitive)
            _feedScroll = TargetTransform.GetComponent<FeedScrollView>();
            if (_feedScroll is null) return;
            _feedScroll.OnViewScrolledDownEvent += OnSwipeUp;

            StartTip();
        }

        private void CheckSwipeDone()
        {
            if (TaskSource.Task.Status == TaskStatus.Running || TaskSource.Task.Status == TaskStatus.WaitingForActivation)
            {
                TaskSource.SetCanceled();
                if (_feedScroll is null) return;
                _feedScroll.OnViewScrolledDownEvent -= OnSwipeUp;
            }
        }

        private void OnSwipeUp()
        {
            if (_feedScroll is not null)
            {
                _feedScroll.OnViewScrolledDownEvent -= OnSwipeUp;
            }
            TaskSource.TrySetResult(true);
        }

        private void OnCloseClick()
        {
            Close();
        }

        private void Close()
        {
            StartCoroutine(FadeRoutine(false, 0f, CompleteTip));
        }

        protected override void PositionateTip() {}

        protected override bool ClickOnTarget(Vector2 clickPosition)
        {
            return true;
        }

        protected override bool TargetOnScreen()
        {
            return true;
        }
        private IEnumerator FadeRoutine(bool fadeIn, float fadeDelay = FADE_DELAY, Action onDoneCallback = null)
        {
            if (fadeDelay > 0f)
            {
                yield return new WaitForSeconds(fadeDelay); // it is needed cause of laggy appearing experience
            }

            float startAlpha = fadeIn ? 0f : 1f;
            float endAlpha = fadeIn ? 1f : 0f;
    
            _canvasGroup.alpha = startAlpha;
    
            var elapsedTime = 0f;
    
            while (elapsedTime < FADE_TIME)
            {
                yield return null;
                elapsedTime += Time.deltaTime;
                _canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / FADE_TIME);
            }
    
            _canvasGroup.alpha = endAlpha;
            onDoneCallback?.Invoke();
        }
    }
}