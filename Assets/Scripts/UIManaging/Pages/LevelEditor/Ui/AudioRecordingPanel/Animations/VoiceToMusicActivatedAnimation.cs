using DG.Tweening;
using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui
{
    internal sealed class VoiceToMusicActivatedAnimation: AudioRecordingPanelTweenAnimation
    {
        [SerializeField] private RectTransform _backgroundTransform;
        [SerializeField] private VoiceIndicatorAnimation _voiceIndicatorAnimation;
        [Header("Settings")]
        [SerializeField] private float _backgroundAnchorPosX = -60;
        [SerializeField] private Vector2 _backgroundSizeDelta = new Vector2(492, 120);

        protected override Sequence BuildSequence()
        {
            return DOTween.Sequence()
                          .Join(_backgroundTransform.DOAnchorPosX(_backgroundAnchorPosX, _duration))
                          .Join(_backgroundTransform.DOSizeDelta(_backgroundSizeDelta, _duration))
                          .Join(_voiceIndicatorAnimation.GetSequence());
        }
    }
}