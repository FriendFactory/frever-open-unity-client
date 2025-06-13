using System.Collections;
using EnhancedUI.EnhancedScroller;
using Modules.LevelManaging.Editing.LevelManagement;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.CreationNewEvent
{
    /// <summary>
    /// Put "+" button just after last event item
    /// </summary>
    internal sealed class CreateNewEventButtonPositionControl: MonoBehaviour
    {
        [SerializeField] private int _space = 40;
        [SerializeField] private EnhancedScroller _eventViewItemsScroller;
        [Inject] private ILevelManager _levelManager;
        [Inject] private PostRecordEditorPageModel _pageModel;
        private Canvas _rootCanvas;
        private float _maxPosX;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        private RectTransform LastEventViewItem => _eventViewItemsScroller.GetCellViewAtDataIndex(_eventViewItemsScroller.EndCellViewIndex).GetComponent<RectTransform>();
        private float Width => GetComponent<RectTransform>().rect.width * CanvasScale;
        private float CanvasScale => _rootCanvas.scaleFactor;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        private void Awake()
        {
            _rootCanvas = _eventViewItemsScroller.GetComponentInParent<Canvas>();
            _maxPosX = transform.position.x;
        }

        private void OnEnable()
        {
            _levelManager.EventDeleted += CorrectPosition;
            if (_pageModel.PostRecordEventsTimelineModel.IsInitialized)
            {
                CorrectPosition();
            }
            else
            {
                _pageModel.PostRecordEventsTimelineModel.Initialized += CorrectPosition;
            }
        }

        private void OnDisable()
        {
            _pageModel.PostRecordEventsTimelineModel.Initialized -= CorrectPosition;
            _levelManager.EventDeleted -= CorrectPosition;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void CorrectPosition()
        {
            StartCoroutine(AdjustPositionAtTheEndOfFrame());
        }
        
        private IEnumerator AdjustPositionAtTheEndOfFrame()
        {
            yield return new WaitForEndOfFrame();
            
            var eventTimelineRightSidePosX = LastEventViewItem.position.x + LastEventViewItem.rect.width / 2f * CanvasScale;
            var posX = eventTimelineRightSidePosX + Width / 2f + _space * CanvasScale;
            posX = Mathf.Clamp(posX, 0, _maxPosX);
            var currentPosition = transform.position;
            currentPosition.x = posX;
            transform.position = currentPosition;
        }
    }
}