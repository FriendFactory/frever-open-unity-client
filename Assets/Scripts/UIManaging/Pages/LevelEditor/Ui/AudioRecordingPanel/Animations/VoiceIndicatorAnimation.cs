using DG.Tweening;
using Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.LevelEditor.Ui
{
    internal sealed class VoiceIndicatorAnimation: AudioRecordingPanelTweenAnimation
    {
        [SerializeField] private Image _voiceEnabledIcon;
        [SerializeField] private Image _voiceDisabledIcon;
        [Header("Settings")]
        [SerializeField] private float _voiceEnabledAlpha = 0f;
        [SerializeField] private float _voiceDisabledAlpha = 1f;

        private void Awake()
        {
            _voiceEnabledIcon.SetActive(true);
            _voiceDisabledIcon.SetActive(true);
        }

        protected override Sequence BuildSequence()
        {
            var halfDuration = _duration * 0.5f;
            var voiceEnabledTween = _voiceEnabledIcon.DOFade(_voiceEnabledAlpha, halfDuration);
            var voiceDisabledTween = _voiceDisabledIcon.DOFade(_voiceDisabledAlpha, halfDuration);

            return DOTween.Sequence()
                          .Append(voiceEnabledTween)
                          .Append(voiceDisabledTween);
        }
    }
}