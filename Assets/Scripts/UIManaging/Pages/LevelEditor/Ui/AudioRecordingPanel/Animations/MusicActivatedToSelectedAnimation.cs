using DG.Tweening;
using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui
{
    internal sealed class MusicActivatedToSelectedAnimation: AudioRecordingPanelTweenAnimation
    {
        [SerializeField] private RectTransform _panelTransform;
        [SerializeField] private RectTransform _backgroundTransform;
        [SerializeField] private SongTitlePanelAnimation songTitlePanelAnimation;
        [Header("Settings")]
        [SerializeField] private Vector2 _panelExpandedSizeDelta = new Vector2(630, 120);
        [SerializeField] private float _backroundAnchorPosX = -60;
        [SerializeField] private Vector2 _backgroundSizeDelta = new Vector2(492, 105);

        protected override Sequence BuildSequence()
        {
            return DOTween.Sequence()
                          .Join(_backgroundTransform.DOAnchorPosX(_backroundAnchorPosX, _duration))
                          .Join(_backgroundTransform.DOSizeDelta(_backgroundSizeDelta, _duration))
                          .Join(_panelTransform.DOSizeDelta(_panelExpandedSizeDelta, _duration))
                          .Join(songTitlePanelAnimation.GetSequence());
        }
    }
}