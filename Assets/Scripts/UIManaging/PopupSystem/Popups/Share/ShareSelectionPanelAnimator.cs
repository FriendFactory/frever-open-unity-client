using Common.Abstract;
using DG.Tweening;
using UnityEngine;

namespace UIManaging.PopupSystem.Popups.Share
{
    public class ShareSelectionPanelAnimator: BaseContextPanel<ShareSelectionPanelModel>
    {
        [SerializeField] private RectTransform _panelTransform;
        [SerializeField] private CanvasGroup _panelCanvasGroup;
        [SerializeField] private RectTransform _listTransform;
        [Header("Animation Parameters")] 
        [SerializeField] private Vector2 _panelTargetSizeDelta = new Vector2(0f, 205f);
        [SerializeField] private Vector2 _listTargetSizeDelta = new Vector2(0f, 1232f);
        [SerializeField] private float _duration = 0.25f;
        [SerializeField] private Ease _ease = Ease.InSine;

        private Sequence _sequence;
        
        private bool IsShown { get; set; }

        protected override void OnInitialized()
        {
            _panelTransform.sizeDelta = new Vector2(0f, 0f);
            _panelCanvasGroup.alpha = 0f;

            InitSequence();

            ContextData.ItemSelectionChanged += OnSelectionChanged;
            IsShown = _panelTransform.sizeDelta.y > 0;
            Refresh(true);
        }

        protected override void BeforeCleanUp()
        {
            _sequence.Kill();

            ContextData.ItemSelectionChanged -= OnSelectionChanged;
        }

        private void OnSelectionChanged(ShareSelectionItemModel _)
        {
            Refresh(false);
        }

        private void Refresh(bool immediate)
        {
            var selectedItemsCount = ContextData.SelectedItems.Count;
            if (selectedItemsCount > 0)
            {
                if (!IsShown)
                {
                    Show(immediate);
                }
            }
            else
            {
                if (IsShown)
                {
                    Hide(immediate);
                }
            }
        }

        private void InitSequence()
        {
            _sequence = DOTween.Sequence()
                               .Join(_panelTransform.DOSizeDelta(_panelTargetSizeDelta, _duration))
                               .Join(_panelCanvasGroup.DOFade(1f, _duration))
                               .Join(_listTransform.DOSizeDelta(_listTargetSizeDelta, _duration))
                               .SetEase(_ease)
                               .SetAutoKill(false)
                               .Pause();
        }

        private void Show(bool immediate)
        {
            if (immediate)
            {
                _sequence.Goto(_sequence.Duration());
            }
            else
            {
                _sequence.PlayForward();
            }
            
            IsShown = true;
        }

        private void Hide(bool immediate)
        {
            if (immediate)
            {
                _sequence.Goto(0);
            }
            else
            {
                _sequence.PlayBackwards();
            }
            
            IsShown = false;
        }
    }
}