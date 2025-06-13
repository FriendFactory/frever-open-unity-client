using System;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace Common.UI
{
    [ExecuteInEditMode]
    public class TextSizer : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private  bool _resizeTextObject = true;
        [SerializeField] private  Vector2 _padding;
        [SerializeField] private bool _useCanvasSizeAsMaxSize;
        [HideIf(nameof(_useCanvasSizeAsMaxSize))]
        [SerializeField] private  Vector2 _maxSize = new Vector2(1000, float.PositiveInfinity);
        [SerializeField] private  Vector2 _minSize;
        [SerializeField] private  Mode _controlAxes = Mode.Both;

        private string _lastText;
        private Mode _lastControlAxes = Mode.None;
        private Vector2 _lastSize;
        private bool _forceRefresh;
        private bool _isTextNull = true;
        private RectTransform _textRectTransform;
        private RectTransform _selfRectTransform;
        private RectTransform _canvasRectTransform;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        private RectTransform CanvasRect
        {
            get
            {
                if (_canvasRectTransform is not null) return _canvasRectTransform;
                _canvasRectTransform = GetComponentInParent<Canvas>(true).GetComponent<RectTransform>();
                return _canvasRectTransform;
            }
        }

        private float MinX
        {
            get
            {
                if ((_controlAxes & Mode.Horizontal) != 0) return _minSize.x;
                return _selfRectTransform.rect.width - _padding.x;
            }
        }

        private float MinY
        {
            get
            {
                if ((_controlAxes & Mode.Vertical) != 0) return _minSize.y;
                return _selfRectTransform.rect.height - _padding.y;
            }
        }

        private float MaxX
        {
            get
            {
                if ((_controlAxes & Mode.Horizontal) != 0)
                {
                    return _useCanvasSizeAsMaxSize? CanvasRect.sizeDelta.x : _maxSize.x;
                }
                return _selfRectTransform.rect.width - _padding.x;
            }
        }

        private float MaxY
        {
            get
            {
                if ((_controlAxes & Mode.Vertical) != 0)
                {
                    return _useCanvasSizeAsMaxSize? CanvasRect.sizeDelta.y : _maxSize.y;
                }
                return _selfRectTransform.rect.height - _padding.y;
            }
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void OnEnable()
        {
            RefreshImmediate();
        }

        private void OnValidate()
        {
            Refresh();
        }

        private void Update()
        {
            if (_isTextNull) return;
            if (!_forceRefresh && _text.text == _lastText && _lastSize == _selfRectTransform.rect.size && _controlAxes == _lastControlAxes) return;

            var preferredSize = _text.GetPreferredValues(MaxX, MaxY);
            preferredSize.x = Mathf.Clamp(preferredSize.x, MinX, MaxX);
            preferredSize.y = Mathf.Clamp(preferredSize.y, MinY, MaxY);
            preferredSize += _padding;

            if ((_controlAxes & Mode.Horizontal) != 0)
            {
                _selfRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredSize.x);
                if (_resizeTextObject)
                {
                    _textRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredSize.x);
                }
            }
            if ((_controlAxes & Mode.Vertical) != 0)
            {
                _selfRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferredSize.y);
                if (_resizeTextObject)
                {
                    _textRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferredSize.y);
                }
            }

            _lastText = _text.text;
            _lastSize = _selfRectTransform.rect.size;
            _lastControlAxes = _controlAxes;
            _forceRefresh = false;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        // Forces a size recalculation on next Update
        // Please note: if you destroy the TMP component or change its reference at runtime
        // you'll have to call Refresh()
        public void Refresh()
        {
            _forceRefresh = true;

            _isTextNull = _text == null;
            if (_text) _textRectTransform = _text.GetComponent<RectTransform>();
            _selfRectTransform = GetComponent<RectTransform>();
        }

        public void RefreshImmediate()
        {
            Refresh();
            Update();
        }

        //---------------------------------------------------------------------
        // Nested
        //---------------------------------------------------------------------

        [Flags]
        private enum Mode
        {
            None        = 0,
            Horizontal  = 0x1,
            Vertical    = 0x2,
            Both        = Horizontal | Vertical
        }
    }
}