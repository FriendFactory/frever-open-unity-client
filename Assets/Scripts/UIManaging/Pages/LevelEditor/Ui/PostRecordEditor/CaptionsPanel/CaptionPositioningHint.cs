using System.Linq;
using DG.Tweening;
using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.CaptionsPanel
{
    internal sealed class CaptionPositioningHint : MonoBehaviour
    {
        [SerializeField] private float _showAlpha = 0.6f;
        [SerializeField] private float _appearingTime = 1;
        [SerializeField] private RectTransformOverlapEventTracker[] _triggerZones;
        [SerializeField] private CanvasGroup _hintAlphaControl;

        private Tween _appearTween;
        private Tween _hideTween;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _appearTween = _hintAlphaControl.DOFade(_showAlpha, _appearingTime).SetAutoKill(false).Pause();
            _hideTween = _hintAlphaControl.DOFade(0, _appearingTime).SetAutoKill(false).Pause();
        }

        private void OnEnable()
        {
            foreach (var triggerZone in _triggerZones)
            {
                triggerZone.OverlappingStarted += Show;
                triggerZone.OverlappingEnded += Hide;
            }

            if (_triggerZones.Any(x => x.IsOverlapping))
            {
                Show();
            }
            else
            {
                HideImmediate();
            }
        }
        
        private void OnDisable()
        {
            foreach (var triggerZone in _triggerZones)
            {
                triggerZone.OverlappingStarted -= Show;
                triggerZone.OverlappingEnded -= Hide;
            }

            HideImmediate();
        }

        private void OnDestroy()
        {
            _appearTween.Kill();
            _hideTween.Kill();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void Show()
        {
            _hideTween.Pause();
            _appearTween.Restart();
        }

        private void Hide()
        {
            _appearTween.Pause();
            _hideTween.Restart();
        }
        
        private void HideImmediate()
        {
            _hideTween.Pause();
            _appearTween.Pause();
            _hintAlphaControl.alpha = 0;
        }
    }
}