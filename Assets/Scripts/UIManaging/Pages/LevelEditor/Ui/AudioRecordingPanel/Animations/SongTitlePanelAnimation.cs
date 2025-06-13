using DG.Tweening;
using Extensions;
using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui
{
    internal sealed class SongTitlePanelAnimation: AudioRecordingPanelTweenAnimation
    {
        [SerializeField] private CanvasGroup _songNonSelectedPanel;
        [SerializeField] private CanvasGroup _songSelectedPanel;
        [Header("Settings")]
        [SerializeField] private float _nonSelectedPanelAlpha = 0f;
        [SerializeField] private float _selectedPanelAlpha = 1f;
        [SerializeField] private bool _reverseAppendOrder = false;

        protected override Sequence BuildSequence()
        {
            var halfDuration = _duration * 0.5f;
            var disableSongNonSelectedPanelTween = _songNonSelectedPanel.DOFade(_nonSelectedPanelAlpha, halfDuration)
                                                                        .OnPlay(() => _songNonSelectedPanel.SetActive(true));
            var enableSongSelectedPanelTween = _songSelectedPanel
                                              .DOFade(_selectedPanelAlpha, _duration)
                                              .OnPlay(() => _songSelectedPanel.SetActive(true));

            var sequence = DOTween.Sequence();

            if (_reverseAppendOrder)
            {
                sequence.Append(enableSongSelectedPanelTween);
                sequence.Append(disableSongNonSelectedPanelTween);
            }
            else
            {
                sequence.Append(disableSongNonSelectedPanelTween);
                sequence.Append(enableSongSelectedPanelTween);
            }

            return sequence;
        }
    }
}