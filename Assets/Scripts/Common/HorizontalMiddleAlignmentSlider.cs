using UnityEngine;
using UnityEngine.UI;

namespace Common
{
    [RequireComponent(typeof(Slider))]
    public class HorizontalMiddleAlignmentSlider : MonoBehaviour
    {
        private Slider _slider;
        private float center = 0.5f;

        private void Awake()
        {
            _slider = GetComponent<Slider>();
        }

        private void LateUpdate()
        {
            UpdateAnchors();
        }

        private void UpdateAnchors()
        {
            _slider.fillRect.anchorMin = new Vector2(Mathf.Clamp(_slider.handleRect.anchorMin.x, 0, center), 0);
            _slider.fillRect.anchorMax = new Vector2(Mathf.Clamp(_slider.handleRect.anchorMin.x, center, 1), 1);
        }
    }
}
