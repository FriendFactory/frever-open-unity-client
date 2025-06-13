using System;
using System.Linq;
using Extensions;
using Modules.LevelManaging.Editing.LevelManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.CaptionsPanel
{
    /// <summary>
    /// Transparent area on the rendering view port (part of the screen), which represents the caption's rect
    /// </summary>
    internal sealed class CaptionProjection : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler
    {
        [Inject] private ILevelManager _levelManager;

        private RectTransform _renderViewPort;
        private readonly Vector3[] _cornersWorldPos = new Vector3[4];
        private readonly Vector2[] _cornersRectTransformLocalPosition = new Vector2[4];

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public long CaptionId { get; private set; }
        public RectTransform RectTransform { get; private set; }

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        
        public event Action<long> Clicked;
        public event Action<long> DragBegin;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        private void Awake()
        {
            RectTransform = GetComponent<RectTransform>();
        }
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            DragBegin?.Invoke(CaptionId);
        }

        public void OnDrag(PointerEventData eventData)
        {
            //empty; without IDragHandler the IBeginDragHandler doesn't work
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void Init(long captionId, RectTransform renderViewPort)
        {
            CaptionId = captionId;
            _renderViewPort = renderViewPort;
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            Clicked?.Invoke(CaptionId);
        }

        public void Refresh()
        {
            var captionAsset = _levelManager.GetCurrentCaptionAssets().FirstOrDefault(x => x.Id == CaptionId);
            gameObject.SetActive(captionAsset != null);
            if (captionAsset == null) return;
            
            var captionRect = captionAsset.CaptionView.RectTransform;
            ProjectRectTransform(captionRect);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void ProjectRectTransform(RectTransform target)
        {
            target.GetWorldCorners(_cornersWorldPos);
            var cam = _levelManager.GetActiveCamera();
            var parentRect = RectTransform.parent.GetComponent<RectTransform>();
            for (var i = 0; i < _cornersWorldPos.Length; i++)
            {
                var viewPortPoint = cam.WorldToViewportPoint(_cornersWorldPos[i]);
                var worldPos = _renderViewPort.GetWorldPositionFromNormalized(viewPortPoint);
                var screenPoint = RectTransformUtility.WorldToScreenPoint(null, worldPos);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPoint, null, out var localPos);
                _cornersRectTransformLocalPosition[i] = localPos;
            }
            
            var width = Vector3.Distance(_cornersRectTransformLocalPosition[2],  _cornersRectTransformLocalPosition[1]);
            var height = Vector3.Distance(_cornersRectTransformLocalPosition[2], _cornersRectTransformLocalPosition[3]);
            RectTransform.localScale = Vector3.one;
            RectTransform.sizeDelta = new Vector2(width, height);;

            RectTransform.pivot = Vector2.zero;
            var anchoredPos = _cornersRectTransformLocalPosition[0];
            RectTransform.anchoredPosition = anchoredPos;
            RectTransform.SetLocalEulerAngleZ(target.localEulerAngles.z);
        }
    }
}
