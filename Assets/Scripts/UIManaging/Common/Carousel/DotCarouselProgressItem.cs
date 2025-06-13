using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using Sirenix.OdinInspector;
#endif

namespace UIManaging.Common.Carousel
{
    [RequireComponent(typeof(Image))]
    public class DotCarouselProgressItem: CarouselProgressItem 
    {
        [SerializeField] private Image _image;
        [SerializeField] private Color _activeColor = Color.white;
        [SerializeField] private Color _inactiveColor = Color.grey;
        
        private bool _isActive;

        private void Awake()
        {
            if (!_image) _image = GetComponent<Image>();
            
            SetActive(false);
        }

        public override void SetActive(bool state)
        {
            _isActive = state;
            _image.color = _isActive ? _activeColor : _inactiveColor;
        }

#if UNITY_EDITOR
        [Button]
        private void ToggleActive()
        {
            _isActive = !_isActive;
            
            SetActive(_isActive);
        }
#endif
    }
}