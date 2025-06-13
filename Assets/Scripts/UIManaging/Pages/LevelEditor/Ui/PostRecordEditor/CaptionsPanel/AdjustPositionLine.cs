using DG.Tweening;
using Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.CaptionsPanel
{
    internal enum LineAxis
    {
        Vertical,
        Horizontal
    }
    
    internal class AdjustPositionLine: MonoBehaviour
    {
        [SerializeField] private float _thresholdDistance = 25;
        [SerializeField] private float _appearingDuration = 0.2f;
        [SerializeField] private LineAxis _lineAxis;
        [SerializeField] private Image _image;
        
        private Tween _showTween;
        private Tween _hideTween;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public bool IsShown { get; private set; }
        public LineAxis Axis => _lineAxis;
        
        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
            
        private void Awake()
        {
            _showTween = _image.DOFade(1, _appearingDuration).SetAutoKill(false);
            _hideTween = _image.DOFade(0, _appearingDuration).SetAutoKill(false);
        }

        private void OnDisable()
        {
            HideImmediate();
        }

        private void OnDestroy()
        {
            _showTween.Kill();
            _hideTween.Kill();
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public bool ShouldStick(Vector2 nextRawPosition)
        {
            var touchLocalPos = ConvertToLocalPosition(transform.parent as RectTransform, nextRawPosition);
            var distance = Mathf.Abs(_lineAxis == LineAxis.Horizontal 
                                         ? transform.localPosition.y - touchLocalPos.y 
                                         : transform.localPosition.x - touchLocalPos.x);
            return distance <= _thresholdDistance;
        }
        
        public void Show()
        {
            if (IsShown) return;
            _hideTween.Pause();
            _showTween.Restart();
            IsShown = true;
        }

        public void Hide()
        {
            if (!IsShown) return;
            _showTween.Pause();
            _hideTween.Restart();
            IsShown = false;
        }

        public void HideImmediate()
        {
            IsShown = false;
            _showTween.Pause();
            _hideTween.Pause();
            _image.SetAlpha(0);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private Vector2 ConvertToLocalPosition(RectTransform targetRectTransform, Vector2 screenPos)
        {    
            RectTransformUtility.ScreenPointToLocalPointInRectangle(targetRectTransform, screenPos, null, out var localPos);
            return localPos;
        }
        
        private void OnDrawGizmosSelected()
        {
            var prevColor = Gizmos.color;
            Gizmos.color = Color.green;

            var scaleFactor = GetComponentInParent<Canvas>().scaleFactor;
            var rect = GetComponent<RectTransform>().rect;
            var size = Axis == LineAxis.Horizontal 
                ? new Vector3(rect.width * scaleFactor, _thresholdDistance) 
                : new Vector3(_thresholdDistance, rect.height * scaleFactor);
            Gizmos.DrawWireCube(transform.position, size);

            Gizmos.color = prevColor;
        }
    }
}