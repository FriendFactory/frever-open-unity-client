using UnityEngine;

namespace UIManaging.Common
{
    internal sealed class ListPositionIndicatorElement : MonoBehaviour
    {
        [SerializeField] private GameObject _activeImage;
        [SerializeField] private GameObject _notActiveImage;

        private int _index;

        public void Initialize(int index)
        {
            _index = index;
        }

        public void Refresh(int currentIndex)
        {
            var shouldBeActive = currentIndex == _index;
            
            _activeImage.SetActive(shouldBeActive);
            _notActiveImage.SetActive(!shouldBeActive);
        }
    }
}