using UnityEngine;

namespace UIManaging.Common.Carousel
{
    public abstract class CarouselProgressView : MonoBehaviour
    {
        public abstract void Initialize(int elementCount);

        public abstract void SetActiveIndex(int elementIndex);
    }
}