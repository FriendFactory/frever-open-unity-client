using System.Linq;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.CaptionsPanel
{
    public sealed class CaptionPanelActivator: MonoBehaviour
    {
        private static readonly float TARGET_CAPTION_DISTANCE_THRESHOLD = 0.2f * Screen.width;
        
        [SerializeField] private CaptionsPanel _captionsPanel;
        [SerializeField] private RectTransform _viewPort;
        [Inject] private CaptionProjectionManager _captionProjectionManager;
        [Inject] private CaptionPanelRotationGestureSource _rotationGestureSource;

        private bool _initialized;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void Enable(bool isEnabled)
        {
            if (isEnabled)
            {
                SubscribeToEvents();
                _rotationGestureSource.SetViewPort(_viewPort);
                _rotationGestureSource.Activate();
            }
            else
            {
                UnsubscribeFromEvents();
                _rotationGestureSource.Deactivate();
            }
            
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void OnExistedCaptionClicked(long captionId)
        {
            if (_captionsPanel.IsShown) return;
            _captionsPanel.StartCaptionTextEditing(captionId);
        }
        
        private void OnCaptionDragged(long captionId)
        {
            if (_captionsPanel.IsShown) return;
            _captionsPanel.StartTransformEditing(captionId);
        }
        
        private void OnRotationBegan(Vector2 rotationPivotPos)
        {
            if (_captionsPanel.IsShown) return;

            var projections = _captionProjectionManager.CurrentProjections;
            var nearestToFocusProjection = projections.Select(x => new
                                                       {
                                                           Distance = Vector2.Distance(x.RectTransform.position, rotationPivotPos),
                                                           Projection = x
                                                       }).OrderBy(x => x.Distance)
                                                      .FirstOrDefault(x => x.Distance < TARGET_CAPTION_DISTANCE_THRESHOLD)
                                                     ?.Projection;
            if (nearestToFocusProjection == null) return;

            var targetCaptionId = nearestToFocusProjection.CaptionId;
            _captionsPanel.StartTransformEditing(targetCaptionId);
        }
        
        private void SubscribeToEvents()
        {
            _captionProjectionManager.ProjectionClicked += OnExistedCaptionClicked;
            _captionProjectionManager.ProjectionDragBegin += OnCaptionDragged;
            _rotationGestureSource.RotationBegan += OnRotationBegan;
        }
        
        private void UnsubscribeFromEvents()
        {
            _captionProjectionManager.ProjectionClicked -= OnExistedCaptionClicked;
            _captionProjectionManager.ProjectionDragBegin -= OnCaptionDragged;
            _rotationGestureSource.RotationBegan -= OnRotationBegan;
        }
    }
}