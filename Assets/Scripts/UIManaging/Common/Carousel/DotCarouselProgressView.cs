using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Common.Carousel
{
    // inspired by https://github.com/Haruma-K/FancyCarouselView
    public class DotCarouselProgressView: CarouselProgressView 
    {
        [SerializeField] private DotCarouselProgressItem _progressItem;
        
        private List<DotCarouselProgressItem> _progressElementInstances = new List<DotCarouselProgressItem>();
        private int _activeIndex = -1;

        private void Awake()
        {
            _progressElementInstances = GetComponentsInChildren<DotCarouselProgressItem>().ToList();
        }
        
        public override void Initialize(int elementCount)
        {
            if (elementCount == 0)
            {
                gameObject.SetActive(false);
                return;
            }
            
            if (GetComponent<HorizontalLayoutGroup>() == null && GetComponent<VerticalLayoutGroup>() == null)
                throw new InvalidOperationException(
                    $"{nameof(DotCarouselProgressView)} requires {nameof(HorizontalLayoutGroup)} or {nameof(VerticalLayoutGroup)}. Make sure it is attached.");

            // Remove all instances if exists
            _progressElementInstances.ForEach(instance => Destroy(instance.gameObject));

            _progressElementInstances = new List<DotCarouselProgressItem>(elementCount);
            for (var i = 0; i < elementCount; i++)
            {
                var instance = Instantiate(_progressItem, transform);
                instance.SetActive(false);
                _progressElementInstances.Add(instance);
            }

            if (_activeIndex != -1) SetActiveIndex(_activeIndex);
        }

        public override void SetActiveIndex(int elementIndex)
        {
            if (_activeIndex != -1 && _progressElementInstances.Count - 1 >= _activeIndex)
            {
                var instance = _progressElementInstances[_activeIndex];
                instance.SetActive(false);
            }

            if (_progressElementInstances.Count - 1 >= elementIndex)
            {
                var instance = _progressElementInstances[elementIndex];
                instance.SetActive(true);
            }

            _activeIndex = elementIndex;
        }
    }
}