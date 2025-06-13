using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.LevelEditor.Ui
{
    internal sealed class MusicSelectedToMusicPreviewedAnimation: AudioRecordingPanelTweenAnimation
    {
        [SerializeField] private RectTransform _panelTransform;
        [SerializeField] private RectTransform _backgroundTransform;
        [SerializeField] private Image _voiceDisabledIcon;
        [Header("Settings")]
        [SerializeField] private Vector2 _panelSizeDelta = new Vector2(432, 120);
        [SerializeField] private float _backroundAnchorPosX = 0f;
        [SerializeField] private Vector2 _backgroundSizeDelta = new Vector2(420, 105);
        [SerializeField] private GameObject _playSongPreviewPanel;
        
        protected override Sequence BuildSequence()
        {
            return DOTween.Sequence()
                          .OnStart(() => _playSongPreviewPanel.SetActive(false))
                          .OnComplete(() => _playSongPreviewPanel.SetActive(false))
                          .OnRewind(() => _playSongPreviewPanel.SetActive(true))
                          .Join(_voiceDisabledIcon.DOFade(0f, _duration))
                          .Join(_backgroundTransform.DOAnchorPosX(_backroundAnchorPosX, _duration))
                          .Join(_backgroundTransform.DOSizeDelta(_backgroundSizeDelta, _duration))
                          .Join(_panelTransform.DOSizeDelta(_panelSizeDelta, _duration));
        }
    }
}