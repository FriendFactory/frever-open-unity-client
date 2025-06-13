using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UIManaging.Pages.UmaEditorPage.Ui
{
    public sealed class ZoomButton : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI _text;
        [SerializeField]
        private float _x2ZoomValue = 35;
        private float _zoomMultiplier;

        public Action<float> ZoomClicked;

        private Button _button;
        private float _maxZoom;
        private float _minZoom;
        private float _currentZoom;
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Setup(float minZoom, float maxZoom)
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(Click);
            _maxZoom = maxZoom;
            _minZoom = minZoom;
            _zoomMultiplier = 1 / (1 - (_x2ZoomValue - _maxZoom) / (_minZoom - _maxZoom));
        }

        public void SetZoom(float currentZoom) {
            _currentZoom = currentZoom;
            UpdateValue();
        }

        public void SetEnabled(bool value) {
            _button.enabled = value;
            var image = _button.image;
            var color = image.color;
            color.a = value ? 1f : 0.5f;
            image.color = color;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void Click()
        {
            var value = Mathf.Ceil((1 - (_currentZoom - _maxZoom) / (_minZoom - _maxZoom)) * _zoomMultiplier * 10) / 10 + 1;
            float destValue = 0;
            if (value == 1)
            {
                destValue = 1;
            }
            else
            {
                destValue = 0;
            }
            var newZoom = _maxZoom - (destValue / _zoomMultiplier - 1) * (_minZoom - _maxZoom);
            newZoom = Mathf.Ceil(newZoom);
            ZoomClicked?.Invoke(newZoom);
        }

        private void UpdateValue()
        {
            var value = Mathf.Ceil((1 - (_currentZoom - _maxZoom) / (_minZoom - _maxZoom)) * _zoomMultiplier * 10) / 10 + 1;
            _text.text = $"{value}x";
        }
    }
}