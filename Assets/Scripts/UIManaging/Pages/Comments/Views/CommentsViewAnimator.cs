using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using TweenExtensions;
using UIManaging.Animated.Behaviours;
using UnityEngine;
using Tween = DG.Tweening.Tween;

namespace UIManaging.Pages.Comments
{
    public class CommentsViewAnimator : MonoBehaviour
    {
        private static readonly int INPUT_FIELD_OFFSET = Application.platform == RuntimePlatform.IPhonePlayer? 325 : 350;
        private const int KEYBOARD_HEIGHT = 625;
        private const double IPHONE6_ASPECT_RATIO = 0.55f;

        public event Action InputFieldSlidingUpStart;
        public event Action InputFieldSlidingUpFinish;
        
        //---------------------------------------------------------------------
        // Fields
        //---------------------------------------------------------------------

        [SerializeField] private GameObject _commentsView;
        [SerializeField] private RectTransform _commentsViewRect;
        [SerializeField] private RectTransform _publishCommentButtonRect;
        [SerializeField] private CanvasGroup _publishCommentButtonCanvasGroup;
        [SerializeField] private RectTransform _inputFieldRectTransform;
        [SerializeField] private RectTransform _listViewRectTransform;
        [SerializeField] private SlideInOutBehaviour _slideViewBehaviour;
        [SerializeField] private SlideInOutBehaviour _slideInputFieldBehaviour;

        private Sequence _showPublishCommentButtonSequence;
        private Tween _slideInputFieldTween;
        private Vector2 _defaultListViewSizeDelta;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public bool InputFieldSlidingDown { get; private set; }
        public bool InputFieldSlidingUp { get; private set; }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        private void Awake()
        {
            InitAnimations();
            _defaultListViewSizeDelta = _listViewRectTransform.sizeDelta;
            _commentsViewRect.localPosition -= new Vector3(0f, _commentsViewRect.rect.height);
        }

        private void InitAnimations()
        {
            _showPublishCommentButtonSequence = DOTween.Sequence()
                                                    .Join(_publishCommentButtonCanvasGroup.DOFade(1f, 0.1f))
                                                    .Join(_publishCommentButtonRect.DOSizeDelta(new Vector2(204, 102), 0.15f))
                                                    .SetAutoKill(false)
                                                    .SetEase(Ease.OutQuad)
                                                    .OnComplete(() =>
                                                    {
                                                        _publishCommentButtonCanvasGroup.interactable = true;
                                                        _publishCommentButtonCanvasGroup.blocksRaycasts = true;
                                                    })
                                                    .Pause();
        }
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void ShowPublishCommentButton()
        {
            _showPublishCommentButtonSequence.PlayByState(true);
        }

        public void HidePublishCommentButton()
        {
            _publishCommentButtonCanvasGroup.interactable = false;
            _publishCommentButtonCanvasGroup.blocksRaycasts = false;
            _showPublishCommentButtonSequence.PlayByState(false);
        }

        public void InitInputFieldSequence()
        {
            var inPosition = new Vector3(0, KEYBOARD_HEIGHT);

            inPosition.y += INPUT_FIELD_OFFSET;
            
            if ((float)Screen.width / Screen.height > IPHONE6_ASPECT_RATIO)
            {
                inPosition.y += INPUT_FIELD_OFFSET;
            }
            
            var outPosition = Vector3.zero;
            _slideInputFieldBehaviour.InitSequence(inPosition, outPosition);
        }
        
        [Button(DisplayParameters = true)]
        public void SlideUpInputField()
        {
            if (InputFieldSlidingUp) return;
            
            InputFieldSlidingUpStart?.Invoke();
            InputFieldSlidingUp = true;
            InputFieldSlidingDown = false;
            _slideInputFieldBehaviour.SlideIn(() =>
            {
                InputFieldSlidingUpFinish?.Invoke();
                InputFieldSlidingUp = false;
            });
        }

        public void HideInputField()
        {
            InputFieldSlidingUp = false;
            InputFieldSlidingDown = false;
            _slideInputFieldBehaviour.Hide();
        }
        [Button]
        public void SlideDownInputField()
        {
            if (InputFieldSlidingDown) return;
            
            InputFieldSlidingDown = true;
            InputFieldSlidingUp = false;
            _slideInputFieldBehaviour.SlideOut(()=>InputFieldSlidingDown = false);
        }
        
        public void SlideUpAnimation()
        {
            _commentsView.SetActive(true);
            InitAnimations();
            _slideViewBehaviour.InitSequence(Vector3.zero, new Vector3(0, -1899.499f,0));
            _slideViewBehaviour.SlideIn();
        }

        public void SlideDownAnimation()
        {
            _slideInputFieldTween?.Pause();
            _slideInputFieldTween?.Rewind();
            _inputFieldRectTransform.anchoredPosition = Vector2.zero;
            _listViewRectTransform.sizeDelta = _defaultListViewSizeDelta;
            InputFieldSlidingDown = false;
            InputFieldSlidingUp = false;
            _slideViewBehaviour.SlideOut(OnSlideDownComplete);
        }

        private void OnSlideDownComplete()
        {
            _commentsView.SetActive(false);
        }
    }
}