using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.CaptionsPanel
{
    internal sealed class CaptionPositionControl: MonoBehaviour
    {
        [SerializeField] private EditableCaptionView _captionView;
        [SerializeField] private RectTransform _dragArea;
        private Vector2 _minPos;
        private Vector2 _maxPos;
        private readonly Vector3[] _cornerWorldCoordinates = new Vector3[4];
        
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public Vector2 CurrentPosition => CaptionRectTransform.position;
        public RectTransform DragArea => _dragArea;
        private RectTransform CaptionRectTransform => _captionView.RectTransform;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void TryToSetPosition(Vector2 pos)
        {
            var posX = Mathf.Clamp(pos.x, _minPos.x, _maxPos.x);
            var posY = Mathf.Clamp(pos.y, _minPos.y, _maxPos.y);
            CaptionRectTransform.position = new Vector3(posX, posY, CaptionRectTransform.position.z);
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        private void OnEnable()
        {
            _dragArea.GetWorldCorners(_cornerWorldCoordinates);
            var draggableZoneLeftBottomCorner = RectTransformUtility.WorldToScreenPoint(null, _cornerWorldCoordinates[0]);
            var size = CaptionRectTransform.rect.size * CaptionRectTransform.GetComponentInParent<Canvas>().scaleFactor;
            _minPos = draggableZoneLeftBottomCorner + size/2f;
            var draggableZoneRightUpperCorner =  RectTransformUtility.WorldToScreenPoint(null, _cornerWorldCoordinates[2]);
            _maxPos = draggableZoneRightUpperCorner - size/2f;
        }
    }
}