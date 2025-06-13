using System;
using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.CaptionsPanel
{
    internal sealed class DeleteCaptionArea : MonoBehaviour
    {
        [SerializeField] private CaptionPositionControl _captionPositionControl;
        [SerializeField] private float _positionOnDragArea = 50;
        [SerializeField] private DeleteCaptionAreaAnimator _animatedBackground;
        [SerializeField] private ProximityTracker _proximityTracker;
        private RectTransform _rectTransform;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public bool IsCaptionInsideArea { get; private set; }

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action CaptionEntered; 
        public event Action CaptionExited; 

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        private void Awake()
        {
            _rectTransform = transform as RectTransform;
        }

        private void OnEnable()
        {
            var position = _rectTransform.localPosition;
            var worldPos = new Vector3[4];
            _captionPositionControl.DragArea.GetWorldCorners(worldPos);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent.GetComponent<RectTransform>(),
                                                                    worldPos[0], null, out var localPos);
            position.y = localPos.y + _positionOnDragArea;
            _rectTransform.localPosition = position;
            _proximityTracker.ProximityEntered += OnCaptionEnteredArea;
            _proximityTracker.ProximityExited += OnCaptionExitedArea;
        }

        private void OnDisable()
        {
            _proximityTracker.ProximityEntered -= OnCaptionEnteredArea;
            _proximityTracker.ProximityExited -= OnCaptionExitedArea;
            IsCaptionInsideArea = false;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void OnCaptionEnteredArea()
        {
            IsCaptionInsideArea = true;
            _animatedBackground.Play();
            CaptionEntered?.Invoke();
        }
        
        private void OnCaptionExitedArea()
        {
            IsCaptionInsideArea = false;
            _animatedBackground.PlayBackward();
            CaptionExited?.Invoke();
        }
    }
}
