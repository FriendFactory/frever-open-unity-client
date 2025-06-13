using Common.Abstract;
using Extensions;
using Laphed.Rx;
using UIManaging.Pages.Common.VideoDetails.VideoAttributes;
using UnityEngine;

namespace UIManaging.Pages.PublishPage.VideoDetails.Attributes
{
    internal abstract class VideoPostAttribute<TValue>: BaseVideoPostAttribute
    {
        protected abstract ReactiveProperty<TValue> Target { get; }
        
        protected override void OnInitialized()
        {
            Target.OnChanged += OnTargetValueChanged;
            IsVisible.OnChanged += OnVisibilityChanged;
            
            OnVisibilityChanged();
        }

        protected override void BeforeCleanUp()
        {
            Target.OnChanged -= OnTargetValueChanged;
            IsVisible.OnChanged -= OnVisibilityChanged;
        }

        private void OnVisibilityChanged()
        {
            if (IsVisible.Value)
            {
                this.SetActive(true);
                OnBecomeVisible();
            }
            else
            {
                this.SetActive(false);
            }
        }

        protected virtual void OnBecomeVisible() {}

        protected abstract void OnTargetValueChanged();
    }
    
    internal abstract class BaseVideoPostAttribute: BaseContextPanel<VideoPostAttributesModel>
    {
        [SerializeField] private VideoAttributeType _type;
        
        public ReactiveProperty<bool> IsVisible { get; protected set; } = new ();
    }
}