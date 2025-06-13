using System.Collections;
using Extensions;
using UI.UIAnimators;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.CaptionsPanel
{
    internal sealed class SizeAdjusterMovementControl : MonoBehaviour
    {
        [SerializeField] private float _autoHideIntervalSec = 2;
        [SerializeField] private MoveUiAnimator _animator;
        [SerializeField] private GameObject _activationArea;

        private Coroutine _autoHideCoroutine;
        
        public bool IsShown { get; private set; }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void Show(bool autoHide)
        {
            if (IsShown) return;
            IsShown = true;
            _animator.PlayShowAnimation(() =>
            {
                if (!autoHide) return;
                RestartAutoHiding();
            });
        }

        public void Hide()
        {
            if (!IsShown) return;
            IsShown = false;
            _animator.PlayHideAnimation();
            StopAutoHiding();
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void OnEnable()
        {
            _activationArea.AddListenerToOnPointerUp(OnPointerUpOnActivationArea);
            _activationArea.AddListenerToOnPointerDown(OnPointerDownOnActivationArea);
        }

        private void OnDisable()
        {
            _activationArea.RemoveListenerFromOnPointerUp(OnPointerUpOnActivationArea);
            _activationArea.RemoveListenerFromOnPointerDown(OnPointerDownOnActivationArea);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void OnPointerDownOnActivationArea(PointerEventData eventData)
        {
            StopAutoHiding();
            if (IsShown) return;
            Show(false);
        }
        
        private void OnPointerUpOnActivationArea(PointerEventData eventData)
        {
            RestartAutoHiding();
        }
        
        private void RestartAutoHiding()
        {
            StopAutoHiding();
            StartAutoHiding();
        }

        private void StartAutoHiding()
        {
            _autoHideCoroutine = StartCoroutine(AutoHide());
        }
        
        private void StopAutoHiding()
        {
            if (_autoHideCoroutine == null) return;
            StopCoroutine(_autoHideCoroutine);
            _autoHideCoroutine = null;
        }
        
        private IEnumerator AutoHide()
        {
            yield return new WaitForSecondsRealtime(_autoHideIntervalSec);
            Hide();
        }
    }
}