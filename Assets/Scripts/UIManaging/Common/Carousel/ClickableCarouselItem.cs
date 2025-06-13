using Abstract;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Common.Carousel
{
    public class ClickableCarouselItem<TCarouselItemModel> : BaseContextDataView<TCarouselItemModel> where TCarouselItemModel: CarouselItemModel
    {
        [SerializeField] protected Image _thumbnail;
        [SerializeField] protected Button _button;
        
        protected override void OnInitialized()
        {
            _button.onClick.AddListener(OnClicked);
        }

        protected override void BeforeCleanup()
        {
            _button.onClick.RemoveListener(OnClicked);
        }

        protected override void OnDestroy()
        {
            if (_thumbnail.sprite)
            {
                Destroy(_thumbnail.sprite);
            }
        }

        protected virtual void OnClicked()
        {
            ContextData.OnItemClicked?.Invoke();
        }
    }
}