using UnityEngine;
using UnityEngine.UI;

namespace Common.UI
{
    public class ImageBreathingAlpha : MonoBehaviour
    {
        [SerializeField] private float _minAlpha = 0.5f;
        [SerializeField] private float _maxAlpha = 1f;
        [SerializeField] private float _speed = 0.5f;
        [Space]
        [SerializeField] private Image[] _images;
        [SerializeField] private CanvasGroup[] _canvasGroups;

        private float _diff;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _diff = _maxAlpha - _minAlpha;
        }

        private void Update()
        {
            var alpha = _minAlpha + Mathf.PingPong(Time.time *  _speed, _diff);

            foreach (var image in _images)
            {
                var color = image.color;
                color.a = alpha;
                image.color = color;
            }

            foreach (var canvasGroup in _canvasGroups)
            {
                canvasGroup.alpha = alpha;
            }
        }
    }
}