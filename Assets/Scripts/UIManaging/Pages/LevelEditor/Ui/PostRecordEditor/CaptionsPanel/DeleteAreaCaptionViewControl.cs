using DG.Tweening;
using Extensions;
using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.CaptionsPanel
{
    internal sealed class DeleteAreaCaptionViewControl: MonoBehaviour
    {
        [SerializeField] private float _scalingFactor = 0.5f;
        [SerializeField] private float _transparencyFactor = 0.5f;
        [SerializeField] private float _changingDurationSec = 0.2f;

        [SerializeField] private DeleteCaptionArea _deleteCaptionArea;
        [SerializeField] private EditableCaptionView _captionView;

        private Sequence _setupEnteredViewStateSequence;
        private Sequence _restoreDefaultViewStateSequence;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        private void Awake()
        {
            _setupEnteredViewStateSequence = DOTween.Sequence()
                                                    .Join(_captionView.RectTransform.DOScale(Vector3.one * _scalingFactor, _changingDurationSec).SetAutoKill(false))
                                                    .Join(_captionView.TextComponent.DOFade(_transparencyFactor, _changingDurationSec).SetAutoKill(false))
                                                    .SetAutoKill(false);
            
            _restoreDefaultViewStateSequence = DOTween.Sequence()
                                                        .Join(_captionView.RectTransform.DOScale(Vector3.one, _changingDurationSec).SetAutoKill(false))
                                                        .Join(_captionView.TextComponent.DOFade(1, _changingDurationSec).SetAutoKill(false))
                                                        .SetAutoKill(false);
        }

        private void OnEnable()
        {
            _deleteCaptionArea.CaptionEntered += OnCaptionEntered;
            _deleteCaptionArea.CaptionExited += OnCaptionExited;
        }
        
        private void OnDisable()
        {
            _deleteCaptionArea.CaptionEntered -= OnCaptionEntered;
            _deleteCaptionArea.CaptionExited -= OnCaptionExited;
            
            _captionView.RectTransform.localScale = Vector3.one;
            _captionView.TextComponent.SetAlpha(1);
        }

        private void OnDestroy()
        {
            _setupEnteredViewStateSequence.Kill();
            _restoreDefaultViewStateSequence.Kill();
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnCaptionEntered()
        {
            _restoreDefaultViewStateSequence.Pause();
            _setupEnteredViewStateSequence.Restart();
        }
        
        private void OnCaptionExited()
        {
            _setupEnteredViewStateSequence.Pause();
            _restoreDefaultViewStateSequence.Restart();
        }
    }
}