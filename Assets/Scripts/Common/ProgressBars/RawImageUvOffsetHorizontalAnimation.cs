using UnityEngine;
using UnityEngine.UI;

namespace Common.ProgressBars
{
    internal sealed class RawImageUvOffsetHorizontalAnimation: MonoBehaviour
    {
        [SerializeField] private float _speed = 2.0f;
        private RawImage _rawImage;
        
        private void Start()
        {
            _rawImage = GetComponent<RawImage>();
        }

        private void Update()
        {
            var uvRect = _rawImage.uvRect;
            uvRect.x += Time.deltaTime * _speed;
            _rawImage.uvRect = uvRect;
        }
    }
}